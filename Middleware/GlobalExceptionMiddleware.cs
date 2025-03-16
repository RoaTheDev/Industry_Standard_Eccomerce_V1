using System.Text.Json;
using Ecommerce_site.Exception;
using Ecommerce_site.Util;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ILogger = Serilog.ILogger;

namespace Ecommerce_site.Middleware;

public class GlobalExceptionMiddleWare(ILogger logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, System.Exception exception,
        CancellationToken cancellationToken)
    {
        var problemDetails = new ProblemDetails();
        logger.Error(exception, "An error occurred: {Message}", exception.Message);
        var (statusCode, specificMessage) = exception switch
        {
            // Database Exceptions
            SqlException sqlEx => HandleSqlException(sqlEx),
            DbUpdateConcurrencyException => (StatusCodes.Status409Conflict,
                "The data was modified by another user. Please refresh and try again."),
            DbUpdateException dbEx => HandleDbUpdateException(dbEx),
            // Application Exceptions
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Access denied, please contact admin."),
            RepoException repoEx => (StatusCodes.Status503ServiceUnavailable, repoEx.Message),
            ServiceException serviceEx => (StatusCodes.Status400BadRequest, serviceEx.Message),
            InvalidOperationException invalidEx => (StatusCodes.Status400BadRequest, invalidEx.Message),
            ArgumentException argEx => (StatusCodes.Status400BadRequest, argEx.Message),
            JsonException jsonEx => (StatusCodes.Status400BadRequest, jsonEx.Message),
            ApiValidationException apiVEx => (StatusCodes.Status400BadRequest, apiVEx.Message),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
        };

        problemDetails.Type = $"https://httpstatuses.com/{statusCode}";
        problemDetails.Status = statusCode;
        problemDetails.Title = GetStatusTitle.GetTitleForStatus(statusCode);
        problemDetails.Detail = specificMessage;
        problemDetails.Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}";
        problemDetails.Extensions.TryAdd("requestId", httpContext.TraceIdentifier);
        var activityFeature = httpContext.Features.Get<IHttpActivityFeature>()!;
        problemDetails.Extensions.TryAdd("traceId", activityFeature.Activity.Id);

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";
        await httpContext.Response.WriteAsync(JsonSerializer.Serialize(problemDetails),
            cancellationToken);
        return true;
    }

    private (int statusCode, string message) HandleDbUpdateException(DbUpdateException dbEx)
    {
        logger.Error(dbEx, "Database update error: {Message}", dbEx.InnerException?.Message);

        if (dbEx.InnerException is SqlException sqlEx)
        {
            return HandleSqlException(sqlEx);
        }

        if (dbEx.Message.Contains("UNIQUE KEY") ||
            dbEx.Message.Contains("FOREIGN KEY") ||
            dbEx.Message.Contains("duplicate key"))
        {
            return (StatusCodes.Status400BadRequest, "The data you submitted conflicts with existing records.");
        }
        return (StatusCodes.Status500InternalServerError, "A database error occurred while processing your request.");
    }

    // only for handling MSSQL error code
    private (int statusCode, string message) HandleSqlException(SqlException sqlEx)
    {
        logger.Error(sqlEx, "SQL error number {Number}: {Message}", sqlEx.Number, sqlEx.Message);

        return sqlEx.Number switch
        {
            -2 or 53 or 258 => (StatusCodes.Status504GatewayTimeout, "The database operation timed out."),
            2 or 40 => (StatusCodes.Status503ServiceUnavailable, "The database is currently unavailable."),
            547 => (StatusCodes.Status400BadRequest, "The operation would violate database constraints."),
            2601 or 2627 => (StatusCodes.Status409Conflict, "A record with this information already exists."),
            18456 or 229 or 916 => (StatusCodes.Status403Forbidden, "Insufficient database permissions."),
            _ => (StatusCodes.Status500InternalServerError, "A database error occurred.")
        };
    }
}
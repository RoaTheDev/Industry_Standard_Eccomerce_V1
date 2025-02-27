using System.Text.Json;
using Ecommerce_site.Exception;
using Ecommerce_site.Util;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce_site.Middleware;

public class GlobalExceptionMiddleWare(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, System.Exception exception,
        CancellationToken cancellationToken)
    {
        var problemDetails = new ProblemDetails();

        var (statusCode, specificMessage) = exception switch
        {
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

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problemDetails
        });
    }
}
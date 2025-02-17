using Ecommerce_site.Dto;
using Ecommerce_site.Exception;
using Microsoft.AspNetCore.Diagnostics;
using ILogger = Serilog.ILogger;

namespace Ecommerce_site.Middleware;

public class GlobalExceptionMiddleware(ILogger logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, System.Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, message) = exception switch
        {
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Access denied, please contact admin."),
            EntityNotFoundException notFoundEx => (StatusCodes.Status404NotFound, notFoundEx.Message),
            EntityAlreadyExistException existEx => (StatusCodes.Status409Conflict, existEx.Message),
            RepoException repoEx => (StatusCodes.Status500InternalServerError, repoEx.Message),
            ServiceException serviceEx => (StatusCodes.Status400BadRequest, serviceEx.Message),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
        };

        logger.Error(exception, "Exception occurred with status code {StatusCode}: {Message}", statusCode, message);

        httpContext.Response.ContentType = "application/json";
        await httpContext.Response.WriteAsJsonAsync(new ApiStandardResponse<string?>(statusCode, message, null),
            cancellationToken: cancellationToken);

        return true;
    }
}
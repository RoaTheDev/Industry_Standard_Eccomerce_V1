using System.Text.Json;
using Ecommerce_site.Dto;
using Ecommerce_site.Exception;
using Microsoft.AspNetCore.Diagnostics;

namespace Ecommerce_site.Middleware;

public class GlobalExceptionMiddleware : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, System.Exception exception,
        CancellationToken cancellationToken)
    {
        ApiStandardResponse<string?> response;

            var (statusCode, message) = exception switch
            {
                UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Access denied, please contact admin."),
                EntityNotFoundException notFoundEx => (StatusCodes.Status404NotFound, notFoundEx.Message),
                EntityAlreadyExistException existEx => (StatusCodes.Status409Conflict, existEx.Message),
                RepoException repoEx => (StatusCodes.Status503ServiceUnavailable, repoEx.Message),
                ServiceException serviceEx => (StatusCodes.Status400BadRequest, serviceEx.Message),
                InvalidOperationException invalidEx => (StatusCodes.Status400BadRequest, invalidEx.Message),
                ArgumentException argEx => (StatusCodes.Status400BadRequest, argEx.Message),
                JsonException jsonEx => (StatusCodes.Status400BadRequest,jsonEx.Message),
                ApiValidationException apiVEx => (StatusCodes.Status400BadRequest,apiVEx.Message),
                _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
            };

            response = new ApiStandardResponse<string?>(statusCode, message, null);

        httpContext.Response.ContentType = "application/json";
        httpContext.Response.StatusCode = response.StatusCode;

        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }


}

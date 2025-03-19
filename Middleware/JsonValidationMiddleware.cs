using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using ILogger = Serilog.ILogger;

namespace Ecommerce_site.Middleware
{
    public class JsonValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public JsonValidationMiddleware(RequestDelegate next, ILogger logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Only validate for HTTP methods that typically have a body

            if (HttpMethods.IsPost(context.Request.Method) ||
                HttpMethods.IsPut(context.Request.Method) ||
                HttpMethods.IsPatch(context.Request.Method))
            {
                // Check if Content-Type indicates JSON
                if (!string.IsNullOrEmpty(context.Request.ContentType) &&
                    context.Request.ContentType.Contains("application/json", StringComparison.OrdinalIgnoreCase))
                {
                    // Enable buffering so the body can be read multiple times
                    context.Request.EnableBuffering();

                    using (var reader = new StreamReader(
                               context.Request.Body,
                               encoding: Encoding.UTF8,
                               detectEncodingFromByteOrderMarks: false,
                               bufferSize: 1024,
                               leaveOpen: true))
                    {
                        var body = await reader.ReadToEndAsync();

                        // Reset the stream position so later middleware can read it
                        context.Request.Body.Position = 0;

                        // Check if body is empty
                        if (string.IsNullOrWhiteSpace(body))
                        {
                            var emptyBodyProblem = new ProblemDetails
                            {
                                Status = StatusCodes.Status400BadRequest,
                                Title = "Empty request body",
                                Detail = "Request body cannot be empty for this operation",
                                Instance = $"{context.Request.Method} {context.Request.Path}"
                            };
                            emptyBodyProblem.Extensions.Add("requestId", context.TraceIdentifier);

                            context.Response.StatusCode = StatusCodes.Status400BadRequest;
                            context.Response.ContentType = "application/problem+json";
                            await context.Response.WriteAsync(JsonSerializer.Serialize(emptyBodyProblem));
                            return;
                        }

                        try
                        {
                            // Try parsing the JSON to catch malformed payloads early
                            JsonDocument.Parse(body);
                        }
                        catch (JsonException ex)
                        {
                            _logger.Error(ex, "Invalid JSON format in request body.");

                            // Create ProblemDetails for invalid JSON format
                            var problemDetails = new ProblemDetails
                            {
                                Status = StatusCodes.Status400BadRequest,
                                Title = "Invalid JSON format",
                                Detail = "The request body contains invalid JSON.",
                                Instance = $"{context.Request.Method} {context.Request.Path}"
                            };
                            problemDetails.Extensions.Add("requestId", context.TraceIdentifier);

                            // Return the error response as JSON
                            context.Response.StatusCode = StatusCodes.Status400BadRequest;
                            context.Response.ContentType = "application/problem+json";
                            var jsonResponse = JsonSerializer.Serialize(problemDetails);
                            await context.Response.WriteAsync(jsonResponse);
                            return;
                        }
                    }
                }

                await _next(context);
            }
        }
    }
}
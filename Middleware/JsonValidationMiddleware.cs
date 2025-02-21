using System.Text;
using System.Text.Json;
using Ecommerce_site.Dto;

namespace Ecommerce_site.Middleware
{
    public class JsonValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<JsonValidationMiddleware> _logger;

        public JsonValidationMiddleware(RequestDelegate next, ILogger<JsonValidationMiddleware> logger)
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

                        // Only try parsing if there is content
                        if (!string.IsNullOrWhiteSpace(body))
                        {
                            try
                            {
                                // Try parsing the JSON to catch malformed payloads early
                                JsonDocument.Parse(body);
                            }
                            catch (JsonException ex)
                            {
                                _logger.LogError(ex, "Invalid JSON format in request body.");

                                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                                context.Response.ContentType = "application/json";
                                var errorResponse = new ApiStandardResponse<object>(400, "Invalid JSON format.", null);
                                var json = JsonSerializer.Serialize(errorResponse);
                                await context.Response.WriteAsync(json);
                                return;
                            }
                        }
                    }
                }
            }

            await _next(context);
        }
    }
}
using System.Diagnostics;
using dotenv.net;
using Ecommerce_site.config;
using Ecommerce_site.filter;
using Ecommerce_site.Middleware;
using Microsoft.AspNetCore.Http.Features;
using Serilog;

DotEnv.Load();
var builder = WebApplication.CreateBuilder(args);
// Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
builder.Host.UseSerilog();
builder.Services.AddProblemDetails(opt =>
{
    opt.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Instance =
            $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
        context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);
        Activity activity = context.HttpContext.Features.Get<IHttpActivityFeature>()!.Activity;
        context.ProblemDetails.Extensions.TryAdd("traceId", activity.Id);
    };
});
builder.Services.FluentValidationConfig();
builder.Services.AddGlobalExceptionHandler();
builder.Services.AddAuthenticationConfig(builder.Configuration);
builder.Services.AddAuthorizationConfig();
builder.Services.AddEmailConfig(builder.Configuration);
builder.Services.LoggingConfig();
builder.Services.AddDbConfig(builder.Configuration);
builder.Services.AddRedisConfig(builder.Configuration);
builder.Services.AddOpenApi();
builder.Services.AddSwaggerConfig();
builder.Services.MapperConfig();
builder.Services.CustomDependencyConfig();

builder.Services.AddControllers(options => options.Filters.Add<FluentValidationFilter>())
    .AddJsonOptions(options =>
    {
        {
            options.AllowInputFormatterExceptionMessages = false;
            options.JsonSerializerOptions.Converters.Add(new CustomJsonConverterFactory());
        }
    });

var app = builder.Build();
app.UseMiddleware<JsonValidationMiddleware>();
app.UseExceptionHandler(_ => { });

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

app.MapGet("/", context =>
{
    context.Response.Redirect("/swagger");
    return Task.CompletedTask;
});
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseStatusCodePages();

app.Run();
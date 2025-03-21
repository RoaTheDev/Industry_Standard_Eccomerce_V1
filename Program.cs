using System.Text.Json;
using dotenv.net;
using Ecommerce_site.config;
using Ecommerce_site.filter;
using Ecommerce_site.Middleware;
using Microsoft.AspNetCore.Mvc;
using Scalar.AspNetCore;
using Serilog;

DotEnv.Load();
var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();
builder.Services.AddProblemDetails();
builder.Services.AddCorsConfig();
builder.Services.FluentValidationConfig();
builder.Services.AddGlobalExceptionHandler();
builder.Services.AddAuthenticationConfig(builder.Configuration);
builder.Services.AddAuthorizationConfig();
builder.Services.AddEmailConfig(builder.Configuration);
builder.Services.LoggingConfig();
builder.Services.AddDbConfig(builder.Configuration);
builder.Services.AddRedisConfig(builder.Configuration);
builder.Services.AddOpenApi();
builder.Services.MapperConfig();
builder.Services.CustomDependencyConfig();

builder.Services.Configure<ApiBehaviorOptions>(options => { options.SuppressModelStateInvalidFilter = true; });
builder.Services.AddControllers(options => { options.Filters.Add<FluentValidationFilter>(); })
    .AddJsonOptions(options =>
    {
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.AllowInputFormatterExceptionMessages = false;
            options.JsonSerializerOptions.Converters.Add(new CustomJsonConverterFactory());
        }
    });

var app = builder.Build();

app.UseCors("ReactAppPolicy");
app.UseMiddleware<JsonValidationMiddleware>();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapGet("/", context =>
{
    context.Response.Redirect("/scalar");
    return Task.CompletedTask;
});
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseStaticFiles();
app.UseHttpsRedirection();
app.Run();
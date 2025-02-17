using dotenv.net;
using Ecommerce_site.config;
using Serilog;

DotEnv.Load();
var builder = WebApplication.CreateBuilder(args);
// Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
builder.Host.UseSerilog();
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
builder.Services.AddControllers();
var app = builder.Build();

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

app.UseHttpsRedirection();

app.Run();
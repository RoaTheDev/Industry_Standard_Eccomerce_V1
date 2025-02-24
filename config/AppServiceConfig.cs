using System.Text;
using Ecommerce_site.Data;
using Ecommerce_site.Middleware;
using FluentEmail.MailKitSmtp;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using StackExchange.Redis;

namespace Ecommerce_site.config;

public static class AppServiceConfig
{


    public static IServiceCollection AddDbConfig(this IServiceCollection service, IConfiguration configuration)
    {
        var dbConStr = configuration["DB_CONNECTION_STR"];
        return service.AddDbContext<EcommerceSiteContext>(opt => opt.UseSqlServer(dbConStr));
    }

    public static IServiceCollection AddRedisConfig(this IServiceCollection service, IConfiguration configuration)
    {
        var redisConStr = configuration["REDIS_CONNECTION"];
        return service.AddStackExchangeRedisCache(opt =>
        {
            opt.Configuration = redisConStr;
            opt.InstanceName = "_Ecom";
            if (opt.Configuration != null)
                opt.ConfigurationOptions = new ConfigurationOptions
                {
                    AbortOnConnectFail = true,
                    DefaultDatabase = 15,
                    EndPoints = { opt.Configuration }
                };
        });
    }

    public static IServiceCollection AddGlobalExceptionHandler(this IServiceCollection services) =>
        services.AddExceptionHandler<GlobalExceptionMiddleware>();

    public static IServiceCollection LoggingConfig(this IServiceCollection services)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File("logs/loggingInfo", rollingInterval: RollingInterval.Day)
            .CreateLogger();
        services.AddSingleton(Log.Logger);
        return services;
    }

    public static IServiceCollection AddSwaggerConfig(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Ecommerce API",
                Version = "v1",
                Description = "API documentation for Ecommerce APP"
            });

            var securityScheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Enter 'Bearer {your token}'",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            };

            options.AddSecurityDefinition("Bearer", securityScheme);

            var securityRequirement = new OpenApiSecurityRequirement
            {
                { securityScheme, new List<string>() }
            };

            options.AddSecurityRequirement(securityRequirement);
        });

        return services;
    }

    public static IServiceCollection AddEmailConfig(this IServiceCollection service, IConfiguration config)
    {
        var username = config["DEFAULT_SMTP_USERNAME"];
        var user = config["SMTP_EMAIL"];
        var password = config["SMTP_PASSWORD"];
        Console.WriteLine(user);
        Console.WriteLine(password);
        var smtpClientOptions = new SmtpClientOptions
        {
            Server = config["SMTP_PROVIDER"],
            Port = Convert.ToInt32(config["SMTP_PORT"]),
            User = config["SMTP_EMAIL"],
            Password = config["SMTP_PASSWORD"],
            //UseSsl = true,
            RequiresAuthentication = true
        };

        service.AddFluentEmail(smtpClientOptions.User, username)
            .AddMailKitSender(smtpClientOptions);
        return service;
    }

    public static IServiceCollection AddAuthenticationConfig(this IServiceCollection service, IConfiguration config)
    {
        service.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    // ValidAlgorithms = [SecurityAlgorithms.HmacSha256Signature], Note to self never put this
                    IssuerSigningKey =
                        new SymmetricSecurityKey(Encoding.ASCII.GetBytes(config["JWT_KEY"] ?? string.Empty)),
                    ValidateIssuerSigningKey = true,
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidateLifetime = true,
                    ValidIssuer = "roa.io",
                    ValidAudience = "ecommerce-app",
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var token = context.Request.Cookies["AuthToken"];
                        if (!string.IsNullOrEmpty(token))
                        {
                            context.Token = token;
                        }

                        return Task.CompletedTask;
                    }
                };
            });
        return service;
    }

    public static IServiceCollection AddAuthorizationConfig(this IServiceCollection service)
    {
        return service.AddAuthorization(opt =>
        {
            opt.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
            opt.AddPolicy("Customer", policy => policy.RequireRole("Customer"));
        });
    }
}
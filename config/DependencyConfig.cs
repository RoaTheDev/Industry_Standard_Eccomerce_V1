using Ecommerce_site.Dto.Request.CustomerRequest;
using Ecommerce_site.Repo;
using Ecommerce_site.Repo.IRepo;
using Ecommerce_site.Service;
using Ecommerce_site.Service.IService;
using Ecommerce_site.Util;
using Ecommerce_site.Validation;
using Ecommerce_site.Validation.CustomerValidation;
using FluentValidation;

namespace Ecommerce_site.config;

public static class DependencyConfig
{
    public static IServiceCollection CustomDependencyConfig(this IServiceCollection service)
    {
        service.AddScoped<RedisCaching>();
        service.AddTransient<OtpGenerator>();
        service.AddSingleton<RazorPageRenderer>();
        service.AddTransient<CustomPasswordHasher>();
        service.AddScoped<JwtGenerator>();
        service.AddSingleton<PaginationMaker>();

        service.AddScoped(typeof(IGenericRepo<>), typeof(GenericRepo<>));
        service.AddScoped<ICustomerService, CustomerService>();

        return service;
    }

    public static IServiceCollection FluentValidationConfig(this IServiceCollection service)
    {
        service.AddTransient<IValidator<CustomerRegisterRequestUap>, CustomerRegisterRequestUapValidator>();
        service.AddTransient<IValidator<CustomerUpdateRequest>, CustomerUpdateRequestValidator>();
        service.AddTransient<IValidator<LoginRequestUap>, LoginRequestValidator>();
        service.AddTransient<IValidator<PasswordChangeRequest>, PasswordChangeRequestValidator>();
        return service;
    }
}
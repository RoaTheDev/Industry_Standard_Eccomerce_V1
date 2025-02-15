using Ecommerce_site.Dto.Request.AddressRequest;
using Ecommerce_site.Dto.Request.CustomerRequest;
using Ecommerce_site.Repo;
using Ecommerce_site.Repo.IRepo;
using Ecommerce_site.Service;
using Ecommerce_site.Service.IService;
using Ecommerce_site.Util;
using Ecommerce_site.Validation.AddressValidation;
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
        service.AddScoped<IAddressService, AddressService>();
        service.AddScoped<ICategoryService, CategoryService>();
        return service;
    }
    public static IServiceCollection MapperConfig(this IServiceCollection service)
    {
        return service.AddAutoMapper(typeof(AppMapper));
    }
    public static IServiceCollection FluentValidationConfig(this IServiceCollection service)
    {
        service.AddTransient<IValidator<CustomerRegisterRequestUap>, CustomerRegisterRequestUapValidator>();
        service.AddTransient<IValidator<CustomerUpdateRequest>, CustomerUpdateRequestValidator>();
        service.AddTransient<IValidator<LoginRequestUap>, LoginRequestValidator>();
        service.AddTransient<IValidator<PasswordChangeRequest>, PasswordChangeRequestValidator>();
        service.AddTransient<IValidator<AddressCreationRequest>, AddressCreateRequestValidation>();
        service.AddTransient<IValidator<AddressUpdateRequest>, AddressUpdateRequestValidation>();

        return service;
    }
}
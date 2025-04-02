using Ecommerce_site.Dto.Request.AddressRequest;
using Ecommerce_site.Dto.Request.CustomerRequest;
using Ecommerce_site.Dto.Request.ProductRequest;
using Ecommerce_site.Repo;
using Ecommerce_site.Repo.IRepo;
using Ecommerce_site.Service;
using Ecommerce_site.Service.IService;
using Ecommerce_site.Service.IService.IProduct;
using Ecommerce_site.Util;
using Ecommerce_site.Util.storage;
using Ecommerce_site.Validation.AddressValidation;
using Ecommerce_site.Validation.CustomerValidation;
using Ecommerce_site.Validation.ProductValidation;
using FluentValidation;

namespace Ecommerce_site.config;

public static class DependencyConfig
{
    public static IServiceCollection CustomDependencyConfig(this IServiceCollection service)
    {
        service.AddSingleton<RazorPageRenderer>();
        service.AddScoped<RedisCaching>();
        service.AddScoped<JwtGenerator>();
        service.AddTransient<OtpGenerator>();
        service.AddTransient<CustomPasswordHasher>();

        service.AddScoped(typeof(IGenericRepo<>), typeof(GenericRepo<>));
        service.AddScoped<ICustomerService, CustomerService>();
        service.AddScoped<IAddressService, AddressService>();
        service.AddScoped<ICategoryService, CategoryService>();
        service.AddScoped<IProductService, ProductService>();
        service.AddScoped<IProductImageService, ProductImageService>();
        service.AddScoped<IProductTagService, ProductTagService>();
        service.AddScoped<IProductFilterService,ProductFilterService>();
        service.AddScoped<ICartService, CartService>();
        service.AddScoped<IOrderService, OrderService>();
        service.AddKeyedTransient<IStorageProvider, LocalStorageProvider>("local");
        // service.AddKeyedTransient<IStorageProvider, AzureBlobStorageProvider>("azure");
        return service;
    }

    public static IServiceCollection MapperConfig(this IServiceCollection service)
    {
        return service.AddAutoMapper(typeof(Program).Assembly);
    }

    public static IServiceCollection FluentValidationConfig(this IServiceCollection service)
    {
        service.AddTransient<IValidator<CustomerRegisterRequestUap>, CustomerRegisterRequestUapValidator>();
        service.AddTransient<IValidator<CustomerUpdateRequest>, CustomerUpdateRequestValidator>();
        service.AddTransient<IValidator<LoginRequestUap>, LoginRequestValidator>();
        service.AddTransient<IValidator<PasswordChangeRequest>, PasswordChangeRequestValidator>();
        service.AddTransient<IValidator<AddressCreationRequest>, AddressCreateRequestValidation>();
        service.AddTransient<IValidator<AddressUpdateRequest>, AddressUpdateRequestValidation>();

        service.AddScoped<IValidator<ProductCreateRequest>, ProductCreateRequestValidator>();
        service.AddScoped<IValidator<ProductUpdateRequest>, ProductUpdateRequestValidator>();

        return service;
    }
}
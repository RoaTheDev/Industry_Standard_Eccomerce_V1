using Ecommerce_site.Dto.Request.ProductRequest;
using Ecommerce_site.Model;
using Ecommerce_site.Repo.IRepo;
using Ecommerce_site.Util;
using FluentValidation;

namespace Ecommerce_site.Validation.ProductValidation;

public class ProductImageAddRequestValidator : AbstractValidator<ProductImageAddRequest>
{
    private readonly IGenericRepo<Product> _productRepo;

    public ProductImageAddRequestValidator(IGenericRepo<Product> productRepo)
    {
        _productRepo = productRepo;
        RuleFor(x => x.ImageUrls)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("At least one image URL/path is required")
            .ForEach(path =>
            {
                path.NotEmpty().WithMessage("Image path cannot be empty")
                    .Must(ValidPath.BeValidPathOrUri).WithMessage("image must be from a valid URL or server path");
            });

        RuleFor(x => x).CustomAsync(ValidProduct);
    }

    private async Task ValidProduct(ProductImageAddRequest request,
        ValidationContext<ProductImageAddRequest> validationContext, CancellationToken _)
    {
        if (!await _productRepo.EntityExistByConditionAsync(p => p.ProductId == request.ProductId))
            validationContext.AddFailure(nameof(Product), "The product does not exist");
    }
}
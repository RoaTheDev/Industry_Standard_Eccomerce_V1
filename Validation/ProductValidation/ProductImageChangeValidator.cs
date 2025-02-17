using Ecommerce_site.Dto.Request.ProductRequest;
using Ecommerce_site.Model;
using Ecommerce_site.Repo.IRepo;
using Ecommerce_site.Util;
using FluentValidation;

namespace Ecommerce_site.Validation.ProductValidation;

public class ProductImageChangeValidator : AbstractValidator<ProductImageChangeRequest>
{
    private readonly IGenericRepo<ProductImage> _pImageRepo;

    public ProductImageChangeValidator(IGenericRepo<ProductImage> pImageRepo)
    {
        _pImageRepo = pImageRepo;

        RuleFor(x => x.ImageUrl).NotEmpty().WithMessage("Image path cannot be empty")
            .Must(ValidPath.BeValidPathOrUri).WithMessage("image must be from a valid URL or server path");
        RuleFor(x => x).CustomAsync(ValidProductImage);
    }

    private async Task ValidProductImage(ProductImageChangeRequest request,
        ValidationContext<ProductImageChangeRequest> validationContext, CancellationToken _)
    {
        if (!await _pImageRepo.EntityExistByConditionAsync(p => p.ProductId == request.ProductId))
            validationContext.AddFailure(nameof(Product), "The product does not exist");
        if (!await _pImageRepo.EntityExistByConditionAsync(p => p.ImageId == request.ImageId))
            validationContext.AddFailure(nameof(ProductImage), "The image does not exist");
    }
}
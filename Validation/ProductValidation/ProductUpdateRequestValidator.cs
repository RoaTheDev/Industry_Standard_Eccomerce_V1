using Ecommerce_site.Dto.Request.ProductRequest;
using Ecommerce_site.Model;
using Ecommerce_site.Repo.IRepo;
using FluentValidation;

namespace Ecommerce_site.Validation.ProductValidation;

public class ProductUpdateRequestValidator : AbstractValidator<ProductUpdateRequest>
{
    private readonly IGenericRepo<Category> _categoryRepo;

    public ProductUpdateRequestValidator(IGenericRepo<Category> categoryRepo)
    {
        _categoryRepo = categoryRepo;

        Include(new ProductRequestValidator<ProductUpdateRequest>());

        RuleFor(x => x).CustomAsync(ValidUpdateAsync);
    }

    private async Task ValidUpdateAsync(ProductUpdateRequest request,
        ValidationContext<ProductUpdateRequest> validationContext, CancellationToken _)
    {
        if (!await _categoryRepo.EntityExistByConditionAsync(c => c.CategoryId == request.CategoryId))
            validationContext.AddFailure("categoryId", "The category does not exist");
    }
}
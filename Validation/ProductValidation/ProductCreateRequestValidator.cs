using Ecommerce_site.Dto.Request.ProductRequest;
using Ecommerce_site.Model;
using Ecommerce_site.Model.Enum;
using Ecommerce_site.Repo.IRepo;
using Ecommerce_site.Util;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_site.Validation.ProductValidation;

public class ProductCreateRequestValidator : AbstractValidator<ProductCreateRequest>
{
    public ProductCreateRequestValidator(
        IGenericRepo<Category> categoryRepository,
        IGenericRepo<User> userRepository)
    {
        Include(new ProductRequestValidator<ProductCreateRequest>());

        RuleFor(x => x).CustomAsync(async (dto, validationContext, _) =>
        {
            if (!await categoryRepository.EntityExistByConditionAsync(c => c.CategoryId == dto.CategoryId))
                validationContext.AddFailure(nameof(Category), "The category does not exist");

            if (!await userRepository.EntityExistByConditionAsync(
                    u => u.UserId == dto.CreateBy && u.Role.RoleName == RoleEnums.Admin.ToString(),
                    include: u => u.Include(ur => ur.Role)))
                validationContext.AddFailure(nameof(User), "Only admins can create products");
        });

        RuleFor(x => x.ImageUrls)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("At least one image URL/path is required")
            .ForEach(path =>
            {
                path.NotEmpty().WithMessage("Image path cannot be empty")
                    .Must(ValidPath.BeValidPathOrUri).WithMessage("image must be from a valid URL or server path");
            });
    }
}
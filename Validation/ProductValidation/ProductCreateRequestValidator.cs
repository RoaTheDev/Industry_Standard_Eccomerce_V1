using Ecommerce_site.Dto.Request.ProductRequest;
using Ecommerce_site.Exception;
using Ecommerce_site.Model;
using Ecommerce_site.Model.Enum;
using Ecommerce_site.Repo.IRepo;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_site.Validation.ProductValidation;

public class ProductCreateRequestValidator : AbstractValidator<ProductCreateRequest>
{
    public ProductCreateRequestValidator(
        IGenericRepo<Category> categoryRepository,
        IGenericRepo<User> userRepository
        )
    {
        Include(new ProductRequestValidator<ProductCreateRequest>());

        RuleFor(x => x).CustomAsync(async (dto, _, _) =>
        {
            if (!await categoryRepository.EntityExistByConditionAsync(c => c.CategoryId == dto.CategoryId))
                throw new EntityNotFoundException(typeof(Category), dto.CategoryId);

            if (!await userRepository.EntityExistByConditionAsync(
                    u => u.UserId == dto.CreateBy && u.Role.RoleName == RoleEnums.Admin.ToString(),
                    include: u => u.Include(ur => ur.Role)))
                throw new EntityNotFoundException(typeof(User), dto.CreateBy);

            // var existingTagCount = await tagRepository.CountByConditionAsync(t => dto.TagIds.Contains(t.TagId));
            // if (existingTagCount != dto.TagIds.Count())
            //     throw new EntityNotFoundException("One or more tags do not exist.");
        });
    }
}
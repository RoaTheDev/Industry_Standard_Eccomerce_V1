using Ecommerce_site.Dto;
using Ecommerce_site.Dto.Request.CategoryRequest;
using Ecommerce_site.Dto.response.CategoryResponse;
using Ecommerce_site.Model;
using Ecommerce_site.Model.Enum;
using Ecommerce_site.Repo.IRepo;
using Ecommerce_site.Service.IService;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_site.Service;

public class CategoryService : ICategoryService
{
    private readonly IGenericRepo<Category> _categoryRepo;
    private readonly IGenericRepo<User> _userRepo;

    public CategoryService(IGenericRepo<Category> categoryRepo, IGenericRepo<User> userRepo)
    {
        _categoryRepo = categoryRepo;
        _userRepo = userRepo;
    }

    public async Task<ApiStandardResponse<CategoryResponse?>> GetCategoryByIdAsync(long id)
    {
        var category =
            await _categoryRepo.GetSelectedColumnsByConditionAsync(
                cate => cate.CategoryId == id && cate.IsActive == true,
                c => new { c.CategoryName, c.Description });

        if (category is null)
            return new ApiStandardResponse<CategoryResponse?>(StatusCodes.Status404NotFound,
                "The category does not exist");
        return new ApiStandardResponse<CategoryResponse?>(StatusCodes.Status200OK, new CategoryResponse
        {
            CategoryName = category.CategoryName, Description = category.Description
        });
    }

    public async Task<ApiStandardResponse<CategoryResponse?>> GetCategoryLikeNameAsync(string name)
    {
        var category =
            await _categoryRepo.GetSelectedColumnsByConditionAsync(
                cate => EF.Functions.Like(cate.CategoryName, name) && cate.IsActive == true,
                cate => new { cate.CategoryName, cate.Description });

        if (category is null)
            return new ApiStandardResponse<CategoryResponse?>(StatusCodes.Status404NotFound,
                "The category does not exist");
        return new ApiStandardResponse<CategoryResponse?>(StatusCodes.Status200OK, new CategoryResponse
        {
            CategoryName = category.CategoryName,
            Description = category.Description
        });
    }

    public async Task<ApiStandardResponse<List<CategoryListResponse>?>> GetCategoryListByIdAsync()
    {
        var categories = await _categoryRepo.GetSelectedColumnsListsAsync(
            cate => new { cate.CategoryId, cate.CategoryName, cate.Description, cate.IsActive });

        if (!categories.Any())
            return new ApiStandardResponse<List<CategoryListResponse>?>(StatusCodes.Status404NotFound,
                "There are no categories");

        List<CategoryListResponse> categoryResponses = new List<CategoryListResponse>();

        foreach (var category in categories)
        {
            categoryResponses.Add(new CategoryListResponse
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName,
                Description = category.Description,
                IsActive = category.IsActive
            });
        }

        return new ApiStandardResponse<List<CategoryListResponse>?>(StatusCodes.Status200OK, categoryResponses);
    }

    public async Task<ApiStandardResponse<CategoryCreateResponse?>> CreateCategoryAsync(CategoryCreateRequest request)
    {
        if (await _categoryRepo.EntityExistByConditionAsync(c =>
                c.CategoryName.ToLower() == request.CategoryName.ToLower()))
            return new ApiStandardResponse<CategoryCreateResponse?>(StatusCodes.Status409Conflict,
                "The category already exist");

        bool isAdmin = await _userRepo.EntityExistByConditionAsync(
            ua => ua.UserId == request.CreateBy && ua.Role.RoleName.ToUpper() == RoleEnums.Admin.ToString().ToUpper(),
            uIn => uIn.Include(ur => ur.Role));

        if (!isAdmin)
            return new ApiStandardResponse<CategoryCreateResponse?>(StatusCodes.Status403Forbidden,
                "Only the admin can create the category");

        var createdCategory = await _categoryRepo.AddAsync(new Category
        {
            CategoryName = request.CategoryName,
            Description = request.Description,
            CreatedBy = request.CreateBy,
        });

        return new ApiStandardResponse<CategoryCreateResponse?>(StatusCodes.Status201Created, new CategoryCreateResponse
        {
            CategoryId = createdCategory.CategoryId,
            CategoryName = createdCategory.CategoryName,
            Description = createdCategory.Description,
            IsActive = createdCategory.IsActive,
            CreatedBy = createdCategory.CreatedBy
        });
    }

    public async Task<ApiStandardResponse<CategoryResponse?>> UpdateCategoryAsync(CategoryUpdateRequest request)
    {
        var category = await _categoryRepo.GetByIdAsync(request.CategoryId);

        if (category is null)
            return new ApiStandardResponse<CategoryResponse?>(StatusCodes.Status404NotFound,
                "The category does not exist");

        bool isAdmin = await _userRepo.EntityExistByConditionAsync(
            ua => ua.UserId == request.UpdatedBy &&
                  ua.Role.RoleName.ToUpper() != RoleEnums.Admin.ToString().ToUpper(),
            uIn => uIn.Include(ur => ur.Role));

        if (!isAdmin)
            return new ApiStandardResponse<CategoryResponse?>(StatusCodes.Status403Forbidden,
                "Only the admin can modify the category");

        if (!string.IsNullOrWhiteSpace(request.CategoryName) && category.CategoryName != request.CategoryName)
            category.CategoryName = request.CategoryName;

        if (!string.IsNullOrWhiteSpace(request.Description) && category.Description != request.Description)
            category.Description = request.Description;

        category.UpdatedAt = DateTime.UtcNow;

        await _categoryRepo.UpdateAsync(category);

        return new ApiStandardResponse<CategoryResponse?>(StatusCodes.Status200OK, new CategoryResponse
        {
            CategoryName = request.CategoryName,
            Description = request.Description
        });
    }

    public async Task<ApiStandardResponse<ConfirmationResponse?>> CategoryStatusChangerAsync(
        CategoryStatusChangeRequest request)
    {
        var user = await _userRepo.GetByConditionAsync(
            ua => ua.UserId == request.AdminId &&
                  ua.Role.RoleName.ToUpper() != RoleEnums.Admin.ToString().ToUpper(),
            uIn =>
                uIn.Include(ur => ur.Role)
                    .Include(ur => ur.CategoryCreatedByNavigations),
            false);

        if (user is null)
            return new ApiStandardResponse<ConfirmationResponse?>(StatusCodes.Status404NotFound,
                $"Only admin can make changes");

        Category? category = null;
        foreach (var c in user.CategoryCreatedByNavigations)
        {
            if (c.CategoryId == request.CategoryId)
                category = c;
        }

        if (category is null)
            return new ApiStandardResponse<ConfirmationResponse?>(StatusCodes.Status404NotFound,
                "The category does not exist");

        category.IsActive = !category.IsActive;
        category.UpdatedAt = DateTime.UtcNow;
        await _categoryRepo.UpdateAsync(category);

        string result = category.IsActive ? "Active" : "Inactive";

        return new ApiStandardResponse<ConfirmationResponse?>(StatusCodes.Status200OK, new ConfirmationResponse
        {
            Message = $"The category status has been change to {result} state."
        });
    }
}
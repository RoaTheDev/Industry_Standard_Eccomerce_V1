using AutoMapper;
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
    private readonly IMapper _mapper;

    public CategoryService(IGenericRepo<Category> categoryRepo, IGenericRepo<User> userRepo, IMapper mapper)
    {
        _categoryRepo = categoryRepo;
        _userRepo = userRepo;
        _mapper = mapper;
    }

    public async Task<ApiStandardResponse<CategoryResponse?>> GetCategoryByIdAsync(long id)
    {
        var category =
            await _categoryRepo.GetSelectedColumnsByConditionAsync(
                cate => cate.CategoryId == id && cate.IsActive,
                c => new CategoryResponse { CategoryName = c.CategoryName, Description = c.Description });

        if (category is null)
            return new ApiStandardResponse<CategoryResponse?>(StatusCodes.Status404NotFound,
                "The category does not exist");
        return new ApiStandardResponse<CategoryResponse?>(StatusCodes.Status200OK, category);
    }

    public async Task<ApiStandardResponse<CategoryResponse?>> GetCategoryLikeNameAsync(string name)
    {
        var category =
            await _categoryRepo.GetSelectedColumnsByConditionAsync(
                cate => EF.Functions.Like(cate.CategoryName, name) && cate.IsActive == true,
                cate => new CategoryResponse { CategoryName = cate.CategoryName, Description = cate.Description });

        if (category is null)
            return new ApiStandardResponse<CategoryResponse?>(StatusCodes.Status404NotFound,
                "The category does not exist");
        return new ApiStandardResponse<CategoryResponse?>(StatusCodes.Status200OK, category);
    }

    public async Task<ApiStandardResponse<List<CategoryListResponse>?>> GetCategoryListByIdAsync()
    {
        var categories = await _categoryRepo.GetSelectedColumnsListsAsync(
            cate => new CategoryListResponse
            {
                CategoryId = cate.CategoryId,
                CategoryName = cate.CategoryName,
                Description = cate.Description,
                IsActive = cate.IsActive
            });
        
        return new ApiStandardResponse<List<CategoryListResponse>?>(StatusCodes.Status200OK, categories);
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
            IsActive = true
        });

        CategoryCreateResponse response = _mapper.Map<CategoryCreateResponse>(createdCategory);
        return new ApiStandardResponse<CategoryCreateResponse?>(StatusCodes.Status201Created, response);
    }

    public async Task<ApiStandardResponse<CategoryResponse?>> UpdateCategoryAsync(long id,
        CategoryUpdateRequest request)
    {
            var category = await _categoryRepo.GetByIdAsync(id);

            if (category is null)
                return new ApiStandardResponse<CategoryResponse?>(StatusCodes.Status404NotFound,
                    "The category does not exist");

            bool isAdmin = await _userRepo.EntityExistByConditionAsync(
                ua => ua.UserId == request.UpdatedBy &&
                      ua.Role.RoleName.ToUpper() == RoleEnums.Admin.ToString().ToUpper(),
                uIn => uIn.Include(ur => ur.Role));

            if (!isAdmin)
                return new ApiStandardResponse<CategoryResponse?>(StatusCodes.Status403Forbidden,
                    "Only the admin can modify the category");

            if (!string.IsNullOrWhiteSpace(request.CategoryName) && category.CategoryName != request.CategoryName)
                category.CategoryName = request.CategoryName;

            if (!string.IsNullOrWhiteSpace(request.Description) && category.Description != request.Description)
                category.Description = request.Description;

            category.UpdatedBy = request.UpdatedBy;
            category.UpdatedAt = DateTime.UtcNow;

            await _categoryRepo.UpdateAsync(category);
            CategoryResponse response = _mapper.Map<CategoryResponse>(category);
            return new ApiStandardResponse<CategoryResponse?>(StatusCodes.Status200OK, response);
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
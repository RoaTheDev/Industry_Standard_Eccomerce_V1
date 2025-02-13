using Ecommerce_site.Dto;
using Ecommerce_site.Dto.Request.CategoryRequest;
using Ecommerce_site.Dto.response.CategoryResponse;
using Ecommerce_site.Exception;
using Ecommerce_site.Model;
using Ecommerce_site.Model.Enum;
using Ecommerce_site.Repo.IRepo;
using Ecommerce_site.Service.IService;
using Microsoft.EntityFrameworkCore;
using ILogger = Serilog.ILogger;

namespace Ecommerce_site.Service;

public class CategoryService : ICategoryService
{
    private readonly ILogger _logger;
    private readonly IGenericRepo<Category> _categoryRepo;
    private readonly IGenericRepo<User> _userRepo;

    public CategoryService(ILogger logger, IGenericRepo<Category> categoryRepo, IGenericRepo<User> userRepo)
    {
        _logger = logger;
        _categoryRepo = categoryRepo;
        _userRepo = userRepo;
    }

    public async Task<ApiStandardResponse<CategoryResponse?>> GetCategoryByIdAsync(long id)
    {
        try
        {
            var category =
                await _categoryRepo.GetSelectedColumnsByConditionAsync(
                    cate => cate.CategoryId == id && cate.IsActive == true,
                    c => new { c.CategoryName, c.Description });

            return new ApiStandardResponse<CategoryResponse?>(StatusCodes.Status200OK, new CategoryResponse
            {
                CategoryName = category.CategoryName, Description = category.Description
            });
        }
        catch (EntityNotFoundException e)
        {
            _logger.Error(e, $"The category with the id {id} does not exist.");
            return new ApiStandardResponse<CategoryResponse?>(StatusCodes.Status404NotFound,
                "The category does not exist",
                null);
        }
    }

    public async Task<ApiStandardResponse<CategoryResponse?>> GetCategoryLikeNameAsync(string name)
    {
        try
        {
            var category =
                await _categoryRepo.GetSelectedColumnsByConditionAsync(
                    cate => EF.Functions.Like(cate.CategoryName, name) && cate.IsActive == true,
                    cate => new { cate.CategoryName, cate.Description });

            return new ApiStandardResponse<CategoryResponse?>(StatusCodes.Status200OK, new CategoryResponse
            {
                CategoryName = category.CategoryName,
                Description = category.Description
            });
        }
        catch (EntityNotFoundException e)
        {
            _logger.Error(e, $"The category with the name like {name} does not exist.");
            return new ApiStandardResponse<CategoryResponse?>(StatusCodes.Status404NotFound,
                "The category does not exist",
                null);
        }
    }

    public async Task<ApiStandardResponse<List<CategoryListResponse>?>> GetCategoryListByIdAsync(long id)
    {
        try
        {
            var categories = await _categoryRepo.GetSelectedColumnsListsByConditionAsync(cate => cate.CategoryId == id,
                cate => new { cate.CategoryName, cate.Description, cate.IsActive });

            if (!categories.Any())
                return new ApiStandardResponse<List<CategoryListResponse>?>(StatusCodes.Status404NotFound,
                    "There are no categories", null);

            List<CategoryListResponse> categoryResponses = new List<CategoryListResponse>();

            foreach (var category in categories)
            {
                categoryResponses.Add(new CategoryListResponse
                {
                    CategoryName = category.CategoryName,
                    Description = category.Description,
                    IsActive = category.IsActive
                });
            }

            return new ApiStandardResponse<List<CategoryListResponse>?>(StatusCodes.Status200OK, categoryResponses);
        }
        catch (System.Exception e)
        {
            _logger.Error(e, "There is an unexpected error ");
            throw;
        }
    }

    public async Task<ApiStandardResponse<CategoryCreateResponse?>> CreateCategoryAsync(CategoryCreateRequest request)
    {
        if (await _categoryRepo.EntityExistByConditionAsync(c =>
                c.CategoryName.ToLower() == request.CategoryName.ToLower()))
            return new ApiStandardResponse<CategoryCreateResponse?>(StatusCodes.Status409Conflict,
                "The category already exist", null);

        bool isAdmin = await _userRepo.EntityExistByConditionAsync(
            ua => ua.UserId == request.CreateBy && ua.Role.RoleName.ToUpper() != RoleEnums.Admin.ToString().ToUpper(),
            uIn => uIn.Include(ur => ur.Role));

        if (!isAdmin)
            return new ApiStandardResponse<CategoryCreateResponse?>(StatusCodes.Status403Forbidden,
                "Only the admin can create the category", null);

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
        try
        {
            Category category = await _categoryRepo.GetByIdAsync(request.CategoryId);

            bool isAdmin = await _userRepo.EntityExistByConditionAsync(
                ua => ua.UserId == request.UpdatedBy &&
                      ua.Role.RoleName.ToUpper() != RoleEnums.Admin.ToString().ToUpper(),
                uIn => uIn.Include(ur => ur.Role));

            if (!isAdmin)
                return new ApiStandardResponse<CategoryResponse?>(StatusCodes.Status403Forbidden,
                    "Only the admin can modify the category", null);

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
        catch (EntityNotFoundException e)
        {
            _logger.Error(e, $"the category with the id {request.CategoryId} does not exist");
            return new ApiStandardResponse<CategoryResponse?>(StatusCodes.Status404NotFound,
                "The category does not exist", null);
        }
    }

    public async Task<ApiStandardResponse<ConfirmationResponse?>> CategoryStatusChangerAsync(
        CategoryStatusChangeRequest request)
    {
        try
        {
            Category category = await _categoryRepo.GetByIdAsync(request.CategoryId);

            bool isAdmin = await _userRepo.EntityExistByConditionAsync(
                ua => ua.UserId == request.AdminId &&
                      ua.Role.RoleName.ToUpper() != RoleEnums.Admin.ToString().ToUpper(),
                uIn => uIn.Include(ur => ur.Role));

            if (!isAdmin)
                return new ApiStandardResponse<ConfirmationResponse?>(StatusCodes.Status403Forbidden,
                    "Only the admin can modify the category", null);

            category.IsActive = !category.IsActive;
            category.UpdatedAt = DateTime.UtcNow;
            await _categoryRepo.UpdateAsync(category);

            string result = category.IsActive ? "Active" : "Inactive";

            return new ApiStandardResponse<ConfirmationResponse?>(StatusCodes.Status200OK, new ConfirmationResponse
            {
                Message = $"The category status has been change to {result} state."
            });
        }
        catch (EntityNotFoundException e)
        {
            _logger.Error(e, $"the category with the id {request.CategoryId} does not exist");
            return new ApiStandardResponse<ConfirmationResponse?>(StatusCodes.Status404NotFound,
                "The category does not exist", null);
        }
    }
}
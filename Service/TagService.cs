using Ecommerce_site.Dto;
using Ecommerce_site.Dto.Request.TagRequest;
using Ecommerce_site.Dto.response.TagResponse;
using Ecommerce_site.Model;
using Ecommerce_site.Model.Enum;
using Ecommerce_site.Repo.IRepo;
using Ecommerce_site.Service.IService;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_site.Service;

public class TagService : ITagService
{
    private readonly IGenericRepo<Tag> _tagRepo;
    private readonly IGenericRepo<User> _userRepo;

    public TagService(IGenericRepo<Tag> tagRepo, IGenericRepo<User> userRepo)
    {
        _tagRepo = tagRepo;
        _userRepo = userRepo;
    }

    public async Task<ApiStandardResponse<PaginatedTagResponse>> GetAllTagsAsync(int cursor, int pageSize)
    {
        if (cursor < 0) cursor = 0;
        if (pageSize < 1) pageSize = 10;
        var tags = await _tagRepo.GetCursorPaginatedSelectedColumnsAsync(t => new AllTagResponse
            {
                TagId = t.TagId,
                TagName = t.TagName
            },
            t => t.TagId,
            cursor,
            pageSize);

        long? nextCursor = null;

        if (tags.Count == pageSize)
        {
            nextCursor = tags.Last().TagId;
        }

        return new ApiStandardResponse<PaginatedTagResponse>(StatusCodes.Status200OK, new PaginatedTagResponse
        {
            Tags = tags,
            PageSize = pageSize,
            NextCursor = nextCursor
        });
    }

    public async Task<ApiStandardResponse<ConfirmationResponse>> CreateTagAsync(long adminId, CreateTagRequest request)
    {
        if (!await _userRepo.EntityExistByConditionAsync(u =>
                    u.UserId == adminId && u.Role.RoleName == RoleEnums.Admin.ToString(),
                include => include.Include(ur => ur.Role)))
        {
            return new ApiStandardResponse<ConfirmationResponse>(StatusCodes.Status404NotFound,
                "Only the admin can modify resources");
        }

        if (await _tagRepo.EntityExistByConditionAsync(t =>
                t.TagName == request.TagName))
        {
            return new ApiStandardResponse<ConfirmationResponse>(StatusCodes.Status409Conflict,
                $"{request.TagName} already exist");
        }

        await _tagRepo.AddAsync(new Tag
        {
            TagName = request.TagName,
        });

        return new ApiStandardResponse<ConfirmationResponse>(StatusCodes.Status201Created, new ConfirmationResponse
        {
            Message = $"{request.TagName} has been created successfully"
        });
    }

    public async Task<ApiStandardResponse<ConfirmationResponse>> UpdateTagAsync(long id, long adminId,
        UpdateTagRequest request)
    {
        if (!await _userRepo.EntityExistByConditionAsync(u =>
                    u.UserId == adminId && u.Role.RoleName == RoleEnums.Admin.ToString(),
                include => include.Include(ur => ur.Role)))
        {
            return new ApiStandardResponse<ConfirmationResponse>(StatusCodes.Status404NotFound,
                "Only the admin can modify resources");
        }

        var tag = await _tagRepo.GetByIdAsync(id);
        if (tag is null)
        {
            return new ApiStandardResponse<ConfirmationResponse>(StatusCodes.Status404NotFound,
                "The tag does not exist");
        }

        tag.TagName = request.TagName;
        tag.UpdatedAt = DateTime.UtcNow;
        await _tagRepo.UpdateAsync(tag);
        return new ApiStandardResponse<ConfirmationResponse>(StatusCodes.Status200OK, new ConfirmationResponse
        {
            Message = $"The tag name has been changed to {tag.TagName}"
        });
    }

    public async Task<ApiStandardResponse<ConfirmationResponse>> DeleteTagAsync(long id, long adminId)
    {
        if (!await _userRepo.EntityExistByConditionAsync(u =>
                    u.UserId == adminId && u.Role.RoleName == RoleEnums.Admin.ToString(),
                include => include.Include(ur => ur.Role)))
        {
            return new ApiStandardResponse<ConfirmationResponse>(StatusCodes.Status404NotFound,
                "Only the admin can modify resources");
        }

        var tag = await _tagRepo.GetByIdAsync(id);
        if (tag is null)
        {
            return new ApiStandardResponse<ConfirmationResponse>(StatusCodes.Status404NotFound,
                "The tag does not exist");
        }

        tag.IsDeleted = true;
        await _tagRepo.UpdateAsync(tag);
        return new ApiStandardResponse<ConfirmationResponse>(StatusCodes.Status200OK, new ConfirmationResponse
        {
            Message = $"{tag.TagName} has been deleted successfully"
        });
    }

    public async Task<ApiStandardResponse<GetTagByIdResponse>> GetTagByIdAsync(long id)
    {
        var tag = await _tagRepo.GetSelectedColumnsByConditionAsync(t => t.TagId == id && !t.IsDeleted, t =>
            new GetTagByIdResponse
            {
                IsDeleted = t.IsDeleted,
                TagId = t.TagId,
                TagName = t.TagName
            });
        if (tag is null)
        {
            return new ApiStandardResponse<GetTagByIdResponse>(StatusCodes.Status404NotFound,
                "The tag does not exist");
        }

        return new ApiStandardResponse<GetTagByIdResponse>(StatusCodes.Status200OK, tag);
    }
}
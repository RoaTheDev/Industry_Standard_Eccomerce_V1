using System.Security.Claims;
using Ecommerce_site.Dto;
using Ecommerce_site.Dto.Request.TagRequest;
using Ecommerce_site.Dto.response.TagResponse;
using Ecommerce_site.Service.IService;
using Ecommerce_site.Util;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce_site.Controller;

[ApiController]
[Route("/api/[controller]")]
public class TagController(ITagService tagService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiStandardResponse<List<AllTagResponse>>>> GetAllTag()
    {
        var response = await tagService.GetAllTagsAsync();
        return response.Success
            ? Ok(response.Data)
            : StatusCode(response.StatusCode, new ProblemDetails
            {
                Status = response.StatusCode,
                Detail = response.Errors!.First().ToString(),
                Title = GetStatusTitle.GetTitleForStatus(response.StatusCode)
            });
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ApiStandardResponse<GetTagByIdResponse>>> GetTagById([FromRoute] long id)
    {
        var response = await tagService.GetTagByIdAsync(id);
        return response.Success
            ? Ok(response.Data)
            : StatusCode(response.StatusCode, new ProblemDetails
            {
                Status = response.StatusCode,
                Detail = response.Errors!.First().ToString(),
                Title = GetStatusTitle.GetTitleForStatus(response.StatusCode)
            });
    }

    [HttpPost]
    public async Task<ActionResult<ApiStandardResponse<ConfirmationResponse>>> CreateTag(CreateTagRequest request)
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var adminId))
        {
            return Unauthorized();
        }

        var response = await tagService.CreateTagAsync(adminId, request);
        return response.Success
            ? Ok(response.Data)
            : StatusCode(response.StatusCode, new ProblemDetails
            {
                Status = response.StatusCode,
                Detail = response.Errors!.First().ToString(),
                Title = GetStatusTitle.GetTitleForStatus(response.StatusCode)
            });
    }

    [HttpPatch("{id:long}")]
    public async Task<ActionResult<ApiStandardResponse<ConfirmationResponse>>> UpdateTag(long id,
        UpdateTagRequest request)
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var adminId))
        {
            return Unauthorized();
        }

        var response = await tagService.UpdateTagAsync(id, adminId, request);
        return response.Success
            ? Ok(response.Data)
            : StatusCode(response.StatusCode, new ProblemDetails
            {
                Status = response.StatusCode,
                Detail = response.Errors!.First().ToString(),
                Title = GetStatusTitle.GetTitleForStatus(response.StatusCode)
            });
    }

    [HttpDelete("{id:long}")]
    public async Task<ActionResult<ApiStandardResponse<ConfirmationResponse>>> DeleteTag(long id)
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var adminId))
        {
            return Unauthorized();
        }

        var response = await tagService.DeleteTagAsync(id, adminId);
        return response.Success
            ? Ok(response.Data)
            : StatusCode(response.StatusCode, new ProblemDetails
            {
                Status = response.StatusCode,
                Detail = response.Errors!.First().ToString(),
                Title = GetStatusTitle.GetTitleForStatus(response.StatusCode)
            });
    }
}
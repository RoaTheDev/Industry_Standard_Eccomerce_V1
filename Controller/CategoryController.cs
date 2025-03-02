using Ecommerce_site.Dto;
using Ecommerce_site.Dto.Request.CategoryRequest;
using Ecommerce_site.Dto.response.CategoryResponse;
using Ecommerce_site.Service.IService;
using Ecommerce_site.Util;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce_site.Controller;

[ApiController]
[Route("api/[controller]")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet("{id:long}/")]
    public async Task<ActionResult<CategoryResponse>> GetCategoryById([FromRoute] long id)
    {
        var response = await _categoryService.GetCategoryByIdAsync(id);
        if (!response.Success)
        {
            return StatusCode(response.StatusCode, new ProblemDetails
            {
                Status = response.StatusCode,
                Title = GetStatusTitle.GetTitleForStatus(response.StatusCode),
                Detail = response.Errors!.First().ToString()
            });
        }

        return Ok(response.Data);
    }

    [HttpGet("search/")]
    public async Task<ActionResult<CategoryResponse>> GetCategoryByName([FromQuery] string name)
    {
        var response = await _categoryService.GetCategoryLikeNameAsync(name);
        if (!response.Success)
        {
            return StatusCode(response.StatusCode, new ProblemDetails
            {
                Status = response.StatusCode,
                Title = GetStatusTitle.GetTitleForStatus(response.StatusCode),
                Detail = response.Errors!.First().ToString()
            });
        }

        return Ok(response.Data);
    }

    [HttpGet]
    public async Task<ActionResult<List<CategoryListResponse>>> GetAllCategory()
    {
        var response = await _categoryService.GetCategoryListByIdAsync();
        if (!response.Success)
        {
            return StatusCode(response.StatusCode, new ProblemDetails
            {
                Status = response.StatusCode,
                Title = GetStatusTitle.GetTitleForStatus(response.StatusCode),
                Detail = response.Errors!.First().ToString()
            });
        }

        return Ok(response.Data);
    }

    [HttpPost]
    public async Task<ActionResult<CategoryCreateResponse>> CreateCategory(
        [FromBody] CategoryCreateRequest request)
    {
        var response = await _categoryService.CreateCategoryAsync(request);
        if (!response.Success)
        {
            return StatusCode(response.StatusCode, new ProblemDetails
            {
                Status = response.StatusCode,
                Title = GetStatusTitle.GetTitleForStatus(response.StatusCode),
                Detail = response.Errors!.First().ToString()
            });
        }

        return CreatedAtAction(nameof(GetCategoryById), new { Id = response.Data!.CategoryId }, response.Data);
    }

    [HttpPatch("{id:long}/")]
    public async Task<ActionResult<CategoryResponse>> UpdateCategory([FromRoute] long id,
        [FromBody] CategoryUpdateRequest request)
    {
        var response = await _categoryService.UpdateCategoryAsync(id, request);
        if (!response.Success)
        {
            return StatusCode(response.StatusCode, new ProblemDetails
            {
                Status = response.StatusCode,
                Detail = response.Errors!.First().ToString(),
                Title = GetStatusTitle.GetTitleForStatus(response.StatusCode)
            });
        }

        return Ok(response.Data);
    }

    [HttpDelete]
    public async Task<ActionResult<ConfirmationResponse>> CategoryStatusChanger(
        [FromBody] CategoryStatusChangeRequest request)
    {
        var response = await _categoryService.CategoryStatusChangerAsync(request);
        if (!response.Success)
        {
            return StatusCode(response.StatusCode, new ProblemDetails
            {
                Status = response.StatusCode,
                Title = GetStatusTitle.GetTitleForStatus(response.StatusCode),
                Detail = response.Errors!.First().ToString()
            });
        }

        return Ok(response.Data);
    }
}
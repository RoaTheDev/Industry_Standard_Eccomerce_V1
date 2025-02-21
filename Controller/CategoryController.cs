using Ecommerce_site.Dto;
using Ecommerce_site.Dto.Request.CategoryRequest;
using Ecommerce_site.Dto.response.CategoryResponse;
using Ecommerce_site.Service.IService;
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
    public async Task<ActionResult<ApiStandardResponse<CategoryResponse>>> GetCategoryById([FromRoute] long id)
    {
        var response = await _categoryService.GetCategoryByIdAsync(id);
        if (response.StatusCode != StatusCodes.Status200OK) return StatusCode(response.StatusCode, response);

        return Ok(response);
    }

    [HttpGet("search/")]
    public async Task<ActionResult<ApiStandardResponse<CategoryResponse>>> GetCategoryByName([FromQuery] string name)
    {
        var response = await _categoryService.GetCategoryLikeNameAsync(name);
        if (response.StatusCode != StatusCodes.Status200OK) return StatusCode(response.StatusCode, response);

        return Ok(response);
    }

    [HttpGet]
    public async Task<ActionResult<ApiStandardResponse<List<CategoryListResponse>>>> GetAllCategory()
    {
        var response = await _categoryService.GetCategoryListByIdAsync();
        if (response.StatusCode != StatusCodes.Status200OK) return StatusCode(response.StatusCode, response);

        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<ApiStandardResponse<CategoryCreateResponse>>> CreateCategory(
        [FromBody] CategoryCreateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiStandardResponse<object>(
                statusCode: StatusCodes.Status400BadRequest,
                errors: ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList(),
                data: null
            ));
        }
        
        var response = await _categoryService.CreateCategoryAsync(request);
        if (response.StatusCode != StatusCodes.Status201Created) return StatusCode(response.StatusCode, response);

        return CreatedAtAction(nameof(GetCategoryById), new { Id = response.Data!.CategoryId }, response);
    }

    [HttpPatch("{id:long}")]
    public async Task<ActionResult<ApiStandardResponse<CategoryResponse>>> UpdateCategory(
        [FromBody] CategoryUpdateRequest request)
    {
        var response = await _categoryService.UpdateCategoryAsync(request);
        if (response.StatusCode != StatusCodes.Status200OK) return StatusCode(response.StatusCode, response);

        return Ok(response);
    }

    [HttpDelete]
    public async Task<ActionResult<ApiStandardResponse<ConfirmationResponse>>> CategoryStatusChanger(
        [FromBody] CategoryStatusChangeRequest request)
    {
        var response = await _categoryService.CategoryStatusChangerAsync(request);
        if (response.StatusCode != StatusCodes.Status200OK) return StatusCode(response.StatusCode, response);

        return Ok(response);
    }
}
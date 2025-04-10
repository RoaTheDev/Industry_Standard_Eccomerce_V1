﻿using Ecommerce_site.Dto;
using Ecommerce_site.Dto.Request.CategoryRequest;
using Ecommerce_site.Dto.response.CategoryResponse;
using Ecommerce_site.Service.IService;
using Ecommerce_site.Util;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce_site.Controller;

[ApiController]
[Route("api/[controller]")]
public class CategoryController(ICategoryService categoryService) : ControllerBase
{
    [HttpGet("{id:long}/")]
    public async Task<ActionResult<CategoryResponse>> GetCategoryById([FromRoute] long id)
    {
        var response = await categoryService.GetCategoryByIdAsync(id);
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
        var response = await categoryService.GetCategoryLikeNameAsync(name);
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
    public async Task<ActionResult<PaginatedCategoryResponse>> GetAllCategory([FromQuery] int cursor,
        [FromQuery] int pageSize)
    {
        var response = await categoryService.GetCategoryListAsync(cursor, pageSize);
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
        var response = await categoryService.CreateCategoryAsync(request);
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
        var response = await categoryService.UpdateCategoryAsync(id, request);
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
    public async Task<ActionResult<ConfirmationResponse>> CategoryStatusChanger( [FromQuery] long categoryId,[FromQuery] long adminId 
        )
    {
        var response = await categoryService.CategoryStatusChangerAsync(categoryId,adminId);
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
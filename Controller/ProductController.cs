using Ecommerce_site.Dto;
using Ecommerce_site.Dto.Request.ProductRequest;
using Ecommerce_site.Dto.response.ProductResponse;
using Ecommerce_site.Service.IService;
using Ecommerce_site.Util;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce_site.Controller;

[ApiController]
[Route("/api/[controller]")]
public class ProductController(IProductService productService)
    : ControllerBase
{
    [HttpGet("{id:long}/")]
    public async Task<ActionResult<ProductByIdResponse>> GetProductById([FromRoute] long id)
    {
        var response = await productService.GetProductByIdAsync(id);
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
    public async Task<ActionResult<ProductByIdResponse>> GetAllProduct(
        [FromQuery] long cursorValue, [FromQuery] int pageSize)
    {
        var response = await productService.GetAllProductAsync(cursorValue, pageSize);
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
    public async Task<ActionResult<ProductCreateResponse>> CreateProduct(
        [FromBody] ProductCreateRequest request)
    {
        var response = await productService.CreateProductAsync(request);
        if (!response.Success)
        {
            return StatusCode(response.StatusCode, new ProblemDetails
            {
                Status = response.StatusCode,
                Title = GetStatusTitle.GetTitleForStatus(response.StatusCode),
                Detail = response.Errors!.First().ToString()
            });
        }

        return CreatedAtAction(nameof(GetProductById), new { Id = response.Data!.ProductId }, response.Data
        );
    }

    [HttpPut("{id:long}/")]
    public async Task<ActionResult<ProductUpdateRequest>> UpdateProduct([FromRoute] long id,
        [FromBody] ProductUpdateRequest request)
    {
        var response = await productService.UpdateProductAsync(id, request);
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

    [HttpDelete("{productId:long}/{imageId:long}/")]
    public async Task<ActionResult<ConfirmationResponse>> DeleteProductImage(
        [FromRoute] long productId, [FromRoute] long imageId)
    {
        var response = await productService.DeleteProductImage(productId, imageId);
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

    [HttpPatch("{id:long}/")]
    public async Task<ActionResult<ProductStatusResponse>> ChangeProductStatus([FromRoute] long id)
    {
        var response = await productService.ChangeProductStatusAsync(id);
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

    [HttpPost("{id:long}/image/")]
    public async Task<ActionResult<ProductImageResponse>> AddProductImage(
        [FromRoute] long id,
        [FromForm] IList<IFormFile> files)
    {
        var response = await productService.AddProductImageAsync(id, files);
        if (!response.Success)
        {
            return StatusCode(response.StatusCode, new ProblemDetails
            {
                Status = response.StatusCode,
                Title = GetStatusTitle.GetTitleForStatus(response.StatusCode),
                Detail = response.Errors!.First().ToString()
            });
        }

        return CreatedAtAction(nameof(GetProductById), new { Id = response.Data!.ProductId }, response.Data);
    }

    [HttpDelete("{id:long}/image/{imageId:long}")]
    public async Task<ActionResult<ProductImageChangeResponse>> ChangeProductImage(
        [FromRoute] long id, [FromRoute] long imageId)
    {
        var response = await productService.ChangeProductImageAsync(id, imageId);
        if (!response.Success)
            return StatusCode(response.StatusCode, new ProblemDetails
            {
                Status = response.StatusCode,
                Title = GetStatusTitle.GetTitleForStatus(response.StatusCode),
                Detail = response.Errors!.First().ToString()
            });
        return Ok(response.Data);
    }

    [HttpPatch("{id:long}/tag")]
    public async Task<ActionResult<ConfirmationResponse>> RemoveProductTag(
        [FromBody] ProductTagRemoveRequest request)
    {
        var response = await productService.ProductTagRemoveAsync(request);

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
}
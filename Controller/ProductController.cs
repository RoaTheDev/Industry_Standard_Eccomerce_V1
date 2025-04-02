using Ecommerce_site.Dto;
using Ecommerce_site.Dto.Request.ProductRequest;
using Ecommerce_site.Dto.response.ProductResponse;
using Ecommerce_site.Service.IService;
using Ecommerce_site.Service.IService.IProduct;
using Ecommerce_site.Util;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce_site.Controller;

[ApiController]
[Route("/api/[controller]")]
public class ProductController(
    IProductService productService,
    IProductImageService productImageService,
    IProductTagService productTagService,
    IProductFilterService productFilterService)
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
    public async Task<ActionResult<PaginatedProductResponse>> GetAllProduct(
        [FromQuery] long cursor = 0, [FromQuery] int pageSize = 10)
    {
        var response = await productService.GetAllProductAsync(cursor, pageSize);
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

    [HttpDelete("{productId:long}/image/{imageId:long}")]
    public async Task<ActionResult<ConfirmationResponse>> DeleteProductImage(
        [FromRoute] long productId, [FromRoute] long imageId)
    {
        var response = await productImageService.DeleteProductImage(productId, imageId);
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
        var response = await productService.UpdateProductStatusAsync(id);
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
        var response = await productImageService.AddProductImageAsync(id, files);
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

    [HttpPatch("{id:long}/image/{imageId:long}")]
    public async Task<ActionResult<ProductImageChangeResponse>> ChangeProductImage(
        [FromRoute] long id, [FromRoute] long imageId, [FromForm] FormFile file)
    {
        var response = await productImageService.UpdateProductImageAsync(id, imageId, file);
        if (!response.Success)
            return StatusCode(response.StatusCode, new ProblemDetails
            {
                Status = response.StatusCode,
                Title = GetStatusTitle.GetTitleForStatus(response.StatusCode),
                Detail = response.Errors!.First().ToString()
            });
        return Ok(response.Data);
    }

    [HttpPost("{id:long}/tag")]
    public async Task<ActionResult<ConfirmationResponse>> AddTagsToProduct([FromRoute] long id,
        AddTagToProductRequest request)
    {
        var response = await productTagService.AddTagsToProduct(id, request);
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

    [HttpPatch("{id:long}/tag")]
    public async Task<ActionResult<ConfirmationResponse>> RemoveProductTag([FromRoute] long id,
        [FromBody] ProductTagRemoveRequest request)
    {
        var response = await productTagService.ProductTagRemoveAsync(id, request);

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

    [HttpGet("best-selling")]
    public async Task<ActionResult<PaginatedProductResponse>> GetBestSellingProduct([FromQuery] long cursor = 0,
        [FromQuery] int pageSize = 10)
    {
        var response = await productFilterService.GetBestSellingProductsAsync(cursor, pageSize);
        return response.Success
            ? Ok(response.Data)
            : StatusCode(response.StatusCode, new ProblemDetails
            {
                Status = response.StatusCode,
                Detail = response.Errors!.First().ToString(),
                Title = GetStatusTitle.GetTitleForStatus(response.StatusCode)
            });
    }

    [HttpGet("new-arrival")]
    public async Task<ActionResult<PaginatedProductResponse>> GetNewArrival([FromQuery] long cursor = 0,
        [FromQuery] int pageSize = 10)
    {
        var response = await productFilterService.GetNewArrivalsAsync(cursor, pageSize);
        return response.Success
            ? Ok(response.Data)
            : StatusCode(response.StatusCode, new ProblemDetails
            {
                Status = response.StatusCode,
                Detail = response.Errors!.First().ToString(),
                Title = GetStatusTitle.GetTitleForStatus(response.StatusCode)
            });
    }

    [HttpGet("filter")]
    public async Task<ActionResult<PaginatedProductResponse>> FilterProduct(
        [FromQuery] ProductFilterRequest request,
        [FromQuery] long cursor = 0,
        [FromQuery] int pageSize = 10)
    {
        var response = await productFilterService.GetFilteredProductsAsync(request, cursor, pageSize);
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
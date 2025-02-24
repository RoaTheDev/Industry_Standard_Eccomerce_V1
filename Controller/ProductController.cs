using Ecommerce_site.Dto;
using Ecommerce_site.Dto.Request.ProductRequest;
using Ecommerce_site.Dto.response.ProductResponse;
using Ecommerce_site.Service.IService;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce_site.Controller;

[ApiController]
[Route("/api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IValidator<ProductCreateRequest> _createValidator;
    private readonly IValidator<ProductUpdateRequest> _updateValidator;

    public ProductController(IProductService productService, IValidator<ProductCreateRequest> createValidator,
        IValidator<ProductUpdateRequest> updateValidator)
    {
        _productService = productService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet("{id:long}/")]
    public async Task<ActionResult<ApiStandardResponse<ProductByIdResponse>>> GetProductById([FromRoute] long id)
    {
        var response = await _productService.GetProductByIdAsync(id);
        if (response.StatusCode != StatusCodes.Status200OK)
            return StatusCode(response.StatusCode, response);
        return Ok(response);
    }

    [HttpGet]
    public async Task<ActionResult<ApiStandardResponse<ProductByIdResponse>>> GetAllProduct(
        [FromQuery] long cursorValue, [FromQuery] int pageSize)
    {
        var response = await _productService.GetAllProductAsync(cursorValue, pageSize);
        if (response.StatusCode != StatusCodes.Status200OK)
            return StatusCode(response.StatusCode, response);
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<ApiStandardResponse<ProductCreateResponse>>> CreateProduct(
        [FromBody] ProductCreateRequest request)
    {
        var validationResult = await _createValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errorList = validationResult.Errors
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(new ApiStandardResponse<ProductCreateResponse?>(
                StatusCodes.Status400BadRequest,
                errorList,
                null));
        }

        var response = await _productService.CreateProductAsync(request);
        if (response.StatusCode != StatusCodes.Status201Created)
            return StatusCode(response.StatusCode, response);

        return CreatedAtAction(nameof(GetProductById), new { Id = response.Data.ProductId }, response
        );
    }

    [HttpPut("{id:long}/")]
    public async Task<ActionResult<ApiStandardResponse<ProductUpdateRequest>>> UpdateProduct(
        [FromBody] ProductUpdateRequest request)
    {
        var validationResult = await _updateValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errorList = validationResult.Errors
                .Select(e => e.ErrorMessage)
                .ToList();

            return BadRequest(new ApiStandardResponse<ProductUpdateResponse?>(
                StatusCodes.Status400BadRequest,
                errorList,
                null));
        }

        var response = await _productService.UpdateProductAsync(request);
        if (response.StatusCode != StatusCodes.Status200OK)
            return StatusCode(response.StatusCode, response);
        return Ok(response);
    }

    [HttpDelete("{productId:long}/{imageId:long}/")]
    public async Task<ActionResult<ApiStandardResponse<ConfirmationResponse>>> DeleteProductImage(
        [FromRoute] long productId, [FromRoute] long imageId)
    {
        var response = await _productService.DeleteProductImage(productId, imageId);
        if (response.StatusCode != StatusCodes.Status200OK)
            return StatusCode(response.StatusCode, response);
        return Ok(response);
    }

    [HttpPatch("{id:long}/")]
    public async Task<ActionResult<ApiStandardResponse<ProductStatusResponse>>> ChangeProductStatus([FromRoute] long id)
    {
        var response = await _productService.ChangeProductStatusAsync(id);
        if (response.StatusCode != StatusCodes.Status200OK)
            return StatusCode(response.StatusCode, response);
        return Ok(response);
    }

    [HttpPost("{id:long}/image/")]
    public async Task<ActionResult<ApiStandardResponse<ProductImageResponse>>> AddProductImage(
        [FromRoute] long id,
        [FromForm] IList<IFormFile> files)
    {
        var response = await _productService.AddProductImageAsync(id, files);
        if (response.StatusCode != StatusCodes.Status201Created)
            return StatusCode(response.StatusCode, response);

        return CreatedAtAction(nameof(GetProductById), new { Id = response.Data!.ProductId }, response);
    }

    [HttpDelete("{id:long}/image/{imageId:long}")]
    public async Task<ActionResult<ApiStandardResponse<ProductImageChangeResponse>>> ChangeProductImage(
        [FromRoute] long id, [FromRoute] long imageId)
    {
        var response = await _productService.ChangeProductImageAsync(id, imageId);
        if (response.StatusCode != StatusCodes.Status200OK)
            return StatusCode(response.StatusCode, response);
        return Ok(response);
    }

    [HttpPatch("{id:long}/tag")]
    public async Task<ActionResult<ApiStandardResponse<ConfirmationResponse>>> RemoveProductTag(
        [FromBody] ProductTagRemoveRequest request)
    {
        var response = await _productService.ProductTagRemoveAsync(request);
        if (response.StatusCode != StatusCodes.Status200OK)
            return StatusCode(response.StatusCode, response);
        return Ok(response);
    }
}
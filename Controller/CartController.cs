using Ecommerce_site.Dto;
using Ecommerce_site.Dto.Request.CartRequest;
using Ecommerce_site.Dto.response.CartResponse;
using Ecommerce_site.Service.IService;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce_site.Controller;

[ApiController]
[Route("/api/{customerId:long}/[controller]")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet]
    public async Task<ActionResult<CartResponse>> GetCartByCustomerId([FromRoute] long customerId)
    {
        var response = await _cartService.GetCartByCustomerIdAsync(customerId);
        if (response.StatusCode != StatusCodes.Status200OK)
            return StatusCode(response.StatusCode, response);
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<ConfirmationResponse>> AddToCart([FromRoute] long customerId,
        [FromBody] AddToCartRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return BadRequest(new ApiStandardResponse<ConfirmationResponse?>(
                StatusCodes.Status400BadRequest,
                "Validation failed"
            ));
        }

        var response = await _cartService.AddToCartAsync(customerId, request);
        if (response.StatusCode != StatusCodes.Status201Created)
            return StatusCode(response.StatusCode, response);

        return CreatedAtAction(nameof(GetCartByCustomerId), new { customerId }, response);
    }

    [HttpDelete("{cartItemId:long}")]
    public async Task<ActionResult<ConfirmationResponse>> RemoveItem([FromRoute] long customerId,
        [FromRoute] long cartItemId)
    {
        var response = await _cartService.RemoveCartItemAsync(customerId, cartItemId);
        return response.StatusCode != StatusCodes.Status200OK
            ? StatusCode(response.StatusCode, response)
            : Ok(response);
    }

    [HttpPatch]
    public async Task<ActionResult<UpdateCartItemResponse>> UpdateCartItem([FromRoute] long customerId,
        [FromBody] CartItemsUpdateRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return BadRequest(new ApiStandardResponse<ConfirmationResponse?>(
                StatusCodes.Status400BadRequest,
                "Validation failed"
            ));
        }

        var response = await _cartService.UpdateCartItemAsync(customerId, request);
        return response.StatusCode != StatusCodes.Status200OK
            ? StatusCode(response.StatusCode, response)
            : Ok(response);
    }
}
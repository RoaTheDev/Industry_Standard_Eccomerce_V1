using Ecommerce_site.Dto;
using Ecommerce_site.Dto.Request.CartRequest;
using Ecommerce_site.Dto.response.CartResponse;
using Ecommerce_site.Service.IService;
using Ecommerce_site.Util;
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
        if (!response.Success)
        {
            return StatusCode(response.StatusCode, new ProblemDetails
            {
                Title = GetStatusTitle.GetTitleForStatus(response.StatusCode),
                Status = response.StatusCode,
                Detail = response.Errors!.First().ToString()
            });
        }

        return Ok(response.Data);
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
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = GetStatusTitle.GetTitleForStatus(StatusCodes.Status400BadRequest),
                Detail = "Validation errors",
                Extensions = { ["errors"] = errors }
            });
        }

        var response = await _cartService.AddToCartAsync(customerId, request);
        if (!response.Success)
        {
            return StatusCode(response.StatusCode, new ProblemDetails
            {
                Status = response.StatusCode,
                Title = GetStatusTitle.GetTitleForStatus(response.StatusCode),
                Detail = response.Errors!.First().ToString()
            });
        }

        return CreatedAtAction(nameof(GetCartByCustomerId), new { customerId }, response.Data);
    }

    [HttpDelete("{cartItemId:long}")]
    public async Task<ActionResult<ConfirmationResponse>> RemoveItem([FromRoute] long customerId,
        [FromRoute] long cartItemId)
    {
        var response = await _cartService.RemoveCartItemAsync(customerId, cartItemId);
        return !response.Success
            ? StatusCode(response.StatusCode, new ProblemDetails
            {
                Status = response.StatusCode,
                Title = GetStatusTitle.GetTitleForStatus(response.StatusCode),
                Detail = response.Errors!.First().ToString()
            })
            : Ok(response.Data);
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
                new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = GetStatusTitle.GetTitleForStatus(StatusCodes.Status400BadRequest),
                    Detail = "Validation errors",
                    Extensions = { ["errors"] = errors }
                }
            ));
        }

        var response = await _cartService.UpdateCartItemAsync(customerId, request);
        return !response.Success
            ? StatusCode(response.StatusCode, new ProblemDetails
            {
                Status = response.StatusCode,
                Title = GetStatusTitle.GetTitleForStatus(response.StatusCode),
                Detail = response.Errors!.First().ToString()
            })
            : Ok(response.Data);
    }
}
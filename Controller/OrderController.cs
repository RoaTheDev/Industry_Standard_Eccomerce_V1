using Ecommerce_site.Dto;
using Ecommerce_site.Dto.Request.OrderRequest;
using Ecommerce_site.Dto.response.OrderResponse;
using Ecommerce_site.Service.IService;
using Ecommerce_site.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce_site.Controller;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet("{customerId:long}/{orderId:long}/")]
    public async Task<ActionResult<ApiStandardResponse<OrderResponse>>> GetOrderById([FromRoute] long customerId,
        [FromRoute] long orderId)
    {
        var response = await _orderService.GetOrderByIdAsync(customerId, orderId);

        return response.Success
            ? Ok(response.Data)
            : StatusCode(response.StatusCode, new ProblemDetails
            {
                Status = response.StatusCode,
                Title = GetStatusTitle.GetTitleForStatus(response.StatusCode),
                Detail = response.Errors!.First().ToString()
            });
    }

    [HttpGet("{customerId:long}")]
    public async Task<ActionResult<ApiStandardResponse<List<OrderResponse>>>> GetOrderList([FromRoute] long customerId)
    {
        var response = await _orderService.GetAllOrderByCustomerIdAsync(customerId);

        return response.Success
            ? Ok(response.Data)
            : StatusCode(response.StatusCode, new ProblemDetails
            {
                Status = response.StatusCode,
                Title = GetStatusTitle.GetTitleForStatus(response.StatusCode),
                Detail = response.Errors!.First().ToString()
            });
    }

    [HttpPost("{customerId:long}/from-cart/")]
    public async Task<ActionResult<ApiStandardResponse<OrderResponse>>> OrderFromCart([FromRoute] long customerId,
        [FromBody] OrderCreateRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(new ApiStandardResponse<OrderResponse>(
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

        var response = await _orderService.OrderCreateFromCartAsync(customerId, request);
        return response.Success
            ? CreatedAtAction(nameof(GetOrderById), new { customerId, orderId = response.Data!.OrderId },
                response.Data)
            : StatusCode(response.StatusCode, new ProblemDetails
            {
                Status = response.StatusCode,
                Title = GetStatusTitle.GetTitleForStatus(response.StatusCode),
                Detail = response.Errors!.First().ToString()
            });
    }

    [HttpPost("{customerId:long}")]
    public async Task<ActionResult<ApiStandardResponse<DirectOrderResponse>>> DirectOrder([FromRoute] long customerId,
        [FromBody] DirectOrderRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(new ApiStandardResponse<DirectOrderResponse>(
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

        var response = await _orderService.DirectOrderCreateAsync(customerId, request);
        return response.Success
            ? CreatedAtAction(nameof(GetOrderById), new { customerId, orderId = response.Data!.OrderId }, response.Data)
            : StatusCode(response.StatusCode, new ProblemDetails
            {
                Status = response.StatusCode,
                Detail = response.Errors!.First().ToString(),
                Title = GetStatusTitle.GetTitleForStatus(response.StatusCode)
            });
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("admin")]
    public async Task<ActionResult<ApiStandardResponse<ConfirmationResponse>>> UpdateOrderStatus(
        OrderStatusChangeRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(new ApiStandardResponse<DirectOrderResponse>(
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

        var response = await _orderService.OrderStatusUpdateAsync(request);
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
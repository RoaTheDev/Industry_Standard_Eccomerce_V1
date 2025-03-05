using Ecommerce_site.Dto;
using Ecommerce_site.Dto.Request.OrderRequest;
using Ecommerce_site.Dto.response.OrderResponse;

namespace Ecommerce_site.Service.IService;

public interface IOrderService
{
    Task<ApiStandardResponse<OrderResponse>> GetOrderByIdAsync(long customerId, long orderId);
    Task<ApiStandardResponse<List<OrderResponse>>> GetAllOrderByCustomerIdAsync(long customerId);
    Task<ApiStandardResponse<OrderResponse>> OrderCreateFromCartAsync(long customerId, OrderCreateRequest request);
    Task<ApiStandardResponse<DirectOrderResponse>> DirectOrderCreateAsync(long customerId, DirectOrderRequest request);

    Task<ApiStandardResponse<ConfirmationResponse>> OrderStatusUpdateAsync(
        OrderStatusChangeRequest request);
}
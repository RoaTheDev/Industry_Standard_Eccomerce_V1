using Ecommerce_site.Dto;
using Ecommerce_site.Dto.Request.OrderRequest;
using Ecommerce_site.Dto.response.OrderResponse;

namespace Ecommerce_site.Service.IService;

public interface IOrderService
{
    Task<ApiStandardResponse<OrderResponse>> GetOrderByIdAsync(long orderId);
    Task<ApiStandardResponse<List<OrderResponse>>> GetAllOrderByCustomerIdAsync(long customerId);
    Task<ApiStandardResponse<OrderResponse>> OrderCreateAsync(OrderCreateRequest request);
    Task<ApiStandardResponse<ConfirmationResponse>> OrderStatusUpdateAsync(OrderStatusChangeRequest request);
}
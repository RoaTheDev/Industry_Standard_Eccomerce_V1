using Ecommerce_site.Dto;
using Ecommerce_site.Dto.Request.CartRequest;
using Ecommerce_site.Dto.response.CartResponse;

namespace Ecommerce_site.Service.IService;

public interface ICartService
{
    Task<ApiStandardResponse<CartResponse?>> GetCartByCustomerIdAsync(long customerId);
    Task<ApiStandardResponse<ConfirmationResponse?>> AddToCartAsync(long customerId, AddToCartRequest request);

    Task<ApiStandardResponse<UpdateCartItemResponse?>> UpdateCartItemAsync(long customerId,
        CartItemsUpdateRequest request);

    Task<ApiStandardResponse<ConfirmationResponse?>> RemoveCartItemAsync(long customerId, long cartItemId);
}
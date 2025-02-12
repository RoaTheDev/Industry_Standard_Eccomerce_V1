using Ecommerce_site.Dto;
using Ecommerce_site.Dto.Request.AddressRequest;
using Ecommerce_site.Dto.response.AddressResponse;

namespace Ecommerce_site.Service.IService;

public interface IAddressService
{
    Task<ApiStandardResponse<AddressResponse?>> GetAddressByAddressIdAsync(long customerId, long addressId);
    Task<ApiStandardResponse<IEnumerable<AddressResponse>?>> GetAddressListByCustomerIdAsync(long customerId);
    Task<ApiStandardResponse<AddressResponse?>> CreateAddressAsync(long customerId, AddressCreationRequest request);
    Task<ApiStandardResponse<AddressResponse?>> UpdateAddressAsync(long customerId,AddressUpdateRequest request);
    Task<ApiStandardResponse<ConfirmationResponse?>> DeleteAddressAsync(long customerId,long  addressId);
}
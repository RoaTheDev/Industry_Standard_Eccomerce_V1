using Ecommerce_site.Dto;
using Ecommerce_site.Dto.Request.AddressRequest;
using Ecommerce_site.Dto.response.AddressResponse;

namespace Ecommerce_site.Service.IService;

public interface IAddressService
{
    Task<ApiStandardResponse<AddressResponse?>> GetAddressByAddressIdAsync(long addressId);
    Task<ApiStandardResponse<IEnumerable<AddressResponse>?>> GetAddressListByCustomerIdAsync(long customerId);
    Task<ApiStandardResponse<AddressResponse?>> CreateAddressAsync(AddressCreationRequest request);
    Task<ApiStandardResponse<AddressResponse?>> UpdateAddressAsync(AddressUpdateRequest request);
    Task<ApiStandardResponse<ConfirmationResponse?>> DeleteAddressAsync(AddressDeleteRequest request);
}
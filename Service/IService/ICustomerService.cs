using Ecommerce_site.Dto;
using Ecommerce_site.Dto.Request.CustomerDto;
using Ecommerce_site.Dto.response.CustomerDto;

namespace Ecommerce_site.Service.IService;

public interface ICustomerService
{
    Task<ApiStandardResponse<CustomerRegisterResponse?>> RegisterCustomerAsync(CustomerRegisterRequestUap request);
    Task<ApiStandardResponse<CustomerGetByIdResponse?>> GetCustomerByIdAsync(long id);

    Task<ApiStandardResponse<CustomerCreationResponse?>> EmailVerification(Guid session,
        EmailVerificationRequest request);

    Task<ApiStandardResponse<LoginResponse?>> LoginAsync(LoginRequest request);
    Task<ApiStandardResponse<CustomerUpdateResponse?>> UpdateCustomerInfoAsync(CustomerUpdateRequest request);
    Task<ApiStandardResponse<ConfirmationResponse?>> PasswordChangeAsync(PasswordChangeRequest request);
}
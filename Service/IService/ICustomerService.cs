using Ecommerce_site.Dto;
using Ecommerce_site.Dto.Request.CustomerRequest;
using Ecommerce_site.Dto.response.CustomerResponse;

namespace Ecommerce_site.Service.IService;

public interface ICustomerService
{
    Task<ApiStandardResponse<CustomerRegisterResponse?>> RegisterCustomerAsync(CustomerRegisterRequestUap request);
    Task<ApiStandardResponse<CustomerGetByIdResponse?>> GetCustomerByIdAsync(long id);

    Task<ApiStandardResponse<CustomerCreationResponse?>> EmailVerification(Guid session,
        EmailVerificationRequest request);

    Task<ApiStandardResponse<LoginResponse?>> LoginAsync(LoginRequestUap requestUap);
    Task<ApiStandardResponse<CustomerUpdateResponse?>> UpdateCustomerInfoAsync(long id, CustomerUpdateRequest request);
    Task<ApiStandardResponse<ConfirmationResponse?>> PasswordChangeAsync(long id, PasswordChangeRequest request);
    Task<ApiStandardResponse<LoginResponse?>> LoginWithGoogle(GoogleLoginRequest request);
    
    Task<ApiStandardResponse<ForgotPasswordResponse?>> ForgotPasswordAsync(ForgotPasswordRequest request);
    Task<ApiStandardResponse<ResetPasswordResponse?>> ResetPasswordAsync(ResetPasswordRequest request, string session);
    Task<ApiStandardResponse<LogoutResponse?>> LogoutAsync(long userId);

    Task<ApiStandardResponse<ConfirmationResponse>> ChangeProfileImage(long id, IFormFile file);
    Task<ApiStandardResponse<ConfirmationResponse?>> LinkGoogleAccount(long userId, string idToken);
    Task<ApiStandardResponse<ConfirmationResponse?>> UnlinkProvider(long userId, string providerId, string providerName);
    Task<ApiStandardResponse<List<AuthProviderResponse>?>> GetLinkedProvidersAsync(long userId);
}
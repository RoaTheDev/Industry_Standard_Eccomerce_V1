using Ecommerce_site.Dto;
using Ecommerce_site.Dto.Request.CustomerDto;
using Ecommerce_site.Dto.response.CustomerDto;

namespace Ecommerce_site.Service.IService;

public interface ICustomerService
{
    Task<ApiStandardResponse<CustomerRegisterResponse?>> RegisterCustomerAsync(CustomerRegisterRequestUap request);
}
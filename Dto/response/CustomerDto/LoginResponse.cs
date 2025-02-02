namespace Ecommerce_site.Dto.response.CustomerDto;

public class LoginResponse
{
    public long CustomerId { get; set; }
    public required string DisplayName { get; set; }
    public required string Message { get; set; }
    public required TokenResponse Token { get; set; }
}
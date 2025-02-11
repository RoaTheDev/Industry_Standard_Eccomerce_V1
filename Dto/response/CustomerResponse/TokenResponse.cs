namespace Ecommerce_site.Dto.response.CustomerResponse;

public class TokenResponse
{
    public required string Token { get; set; }
    public required string ExpiresAt { get; set; }
}
namespace Ecommerce_site.Dto.response.CustomerDto;

public class TokenResponse
{
    public required string Token { get; set; }
    public required string ExpiresAt { get; set; }
}
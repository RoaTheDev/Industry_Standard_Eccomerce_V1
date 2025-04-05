namespace Ecommerce_site.Dto.response.CustomerResponse;

public class CustomerCreationResponse
{
    public required long UserId { get; set; }
    public required string DisplayName { get; set; }

    public required string Email { get; set; }
    public required TokenResponse Token { get; set; }
}
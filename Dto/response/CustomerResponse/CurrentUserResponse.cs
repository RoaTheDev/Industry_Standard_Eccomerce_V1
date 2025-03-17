namespace Ecommerce_site.Dto.response.CustomerResponse;

public class CurrentUserResponse
{
    public required long CustomerId { get; set; }
    public required string DisplayName { get; set; } 
    public required string Email { get; set; }
}
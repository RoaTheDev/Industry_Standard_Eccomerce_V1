namespace Ecommerce_site.Dto.Request.CustomerDto;

public class LoginRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}
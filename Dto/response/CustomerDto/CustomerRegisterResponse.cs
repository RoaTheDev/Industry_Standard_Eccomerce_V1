namespace Ecommerce_site.Dto.response.CustomerDto;

public class CustomerRegisterResponse
{
    public required Guid Session { get; set; }
    public required string SignUpSessionExpAt { get; set; }
}
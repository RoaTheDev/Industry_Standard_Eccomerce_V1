namespace Ecommerce_site.Dto.Request.CustomerDto;

public class PasswordChangeRequest
{
    public required long CustomerId { get; set; }
    public required string CurrentPassword { get; set; }
    public required string NewPassword { get; set; }
    public required string ConfirmNewPassword { get; set; }
}
namespace Ecommerce_site.Dto.Request.CustomerRequest;

public class PasswordChangeRequest
{
    public required string CurrentPassword { get; set; }
    public required string NewPassword { get; set; }
    public required string ConfirmNewPassword { get; set; }
}
namespace Ecommerce_site.Dto.Request.CustomerRequest;

public class PasswordChangeRequest
{
    public  string? CurrentPassword { get; set; }
    public  string? NewPassword { get; set; }
    public  string? ConfirmNewPassword { get; set; }
}
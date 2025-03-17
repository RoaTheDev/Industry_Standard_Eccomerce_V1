namespace Ecommerce_site.Email.Msg;

public class PasswordResetMsg
{
    public required string ResetLink { get; set; }
    public int ExpirationMinutes { get; set; }
}
namespace Ecommerce_site.Email.Msg;

public class EmailVerificationMsg
{
    public required uint VerificationCode { get; set; }
    public required int VerificationExpTime { get; set; }
}
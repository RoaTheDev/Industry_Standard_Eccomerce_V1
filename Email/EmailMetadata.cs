namespace Ecommerce_site.Email;

public class EmailMetadata
{
    public required string ToAddress { get; set; }
    public required string Subject { get; set; }
    public required string TemplatePath { get; set; }
}
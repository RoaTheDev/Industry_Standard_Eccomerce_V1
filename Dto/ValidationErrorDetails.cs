namespace Ecommerce_site.Dto;

public class ValidationErrorDetails
{
    public required string Field { get; set; }
    public required string Reason { get; set; }
}
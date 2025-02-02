namespace Ecommerce_site.Cache;

public class UserCreationCache
{
    public required string FirstName { get; set; }
    public required string MiddleName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required DateOnly Dob { get; set; }
    public required string PhoneNumber { get; set; }
}
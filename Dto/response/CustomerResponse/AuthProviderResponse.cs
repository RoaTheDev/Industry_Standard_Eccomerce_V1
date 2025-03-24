namespace Ecommerce_site.Dto.response.CustomerResponse;

public  class AuthProviderResponse
{
    public string ProviderId { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public DateTime LinkedAt { get; set; }
}

public class LinkGoogleRequest
{
    public required string IdToken { get; set; }
}
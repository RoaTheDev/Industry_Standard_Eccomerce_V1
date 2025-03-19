using System.Text.Json.Serialization;
using Ecommerce_site.config.Rule;

namespace Ecommerce_site.Dto.Request.CustomerRequest;

public class LoginRequestUap
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
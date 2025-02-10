namespace Ecommerce_site.Model;

public  class AuthProvider
{
    public long AuthId { get; set; }

    public long UserId { get; set; }

    public string ProviderName { get; set; } = null!;

    public string AuthProviderId { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual User User { get; set; } = null!;
}

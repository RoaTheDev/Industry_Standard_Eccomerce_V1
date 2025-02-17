namespace Ecommerce_site.Model;

public partial class Tag
{
    public long TagId { get; set; }

    public string Tag1 { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}

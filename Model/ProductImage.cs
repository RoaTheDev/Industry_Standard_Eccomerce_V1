namespace Ecommerce_site.Model;

public  class ProductImage
{
    public long ImageId { get; set; }

    public long ProductId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public bool IsPrimary { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Product Product { get; set; } = null!;
}

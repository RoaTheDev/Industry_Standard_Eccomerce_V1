namespace Ecommerce_site.Model;

public  class Category
{
    public long CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public string Description { get; set; } = null!;

    public bool IsActive { get; set; }

    public long CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}

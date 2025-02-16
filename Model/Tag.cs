namespace Ecommerce_site.Model;

public class Tag
{
    public long TagId { get; set; }
    public string TagName { get; set; } = null!;
    public DateTime CreateAt { get; set; }
    public DateTime UpdateAt { get; set; }
    public bool IsDeleted { get; set; }
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
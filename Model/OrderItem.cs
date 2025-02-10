namespace Ecommerce_site.Model;

public class OrderItem
{
    public long OrderItemId { get; set; }

    public long OrderId { get; set; }

    public long ProductId { get; set; }

    public long Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal Discount { get; set; }

    public decimal TotalPrice { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual Order Order { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
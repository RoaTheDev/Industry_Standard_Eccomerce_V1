namespace Ecommerce_site.Model;

public  class CartItem
{
    public long CartItemId { get; set; }

    public long CartId { get; set; }

    public long ProductId { get; set; }

    public long Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal Discount { get; set; }

    public decimal TotalPrice { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Cart Cart { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}

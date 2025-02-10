namespace Ecommerce_site.Model;

public  class Cart
{
    public long CartId { get; set; }

    public long CustomerId { get; set; }

    public bool IsCheckout { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual Customer Customer { get; set; } = null!;
}

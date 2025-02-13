namespace Ecommerce_site.Model;

public  class Order
{
    public long OrderId { get; set; }

    public long CustomerId { get; set; }

    public string OrderNumber { get; set; } = null!;

    public DateTime OrderDate { get; set; }

    public string OrderStatus { get; set; } = null!;

    public long BillingAddressId { get; set; }

    public long ShippingAddressId { get; set; }

    public decimal TotalBasedAmount { get; set; }

    public decimal ShippingCost { get; set; }

    public decimal TotalAmount { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Address BillingAddress { get; set; } = null!;

    public virtual ICollection<Cancellation> Cancellations { get; set; } = new List<Cancellation>();

    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual Address ShippingAddress { get; set; } = null!;
}

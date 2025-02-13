namespace Ecommerce_site.Model;

public  class Payment
{
    public long PaymentId { get; set; }

    public long OrderId { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public string? TransactionId { get; set; }

    public decimal Amount { get; set; }

    public DateTime PaymentDate { get; set; }

    public string PaymentStatus { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual Refund? Refund { get; set; }
}

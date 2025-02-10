namespace Ecommerce_site.Model;

public class Refund
{
    public int RefundId { get; set; }

    public int CancellationId { get; set; }

    public long PaymentId { get; set; }

    public decimal Amount { get; set; }

    public string RefundStatus { get; set; } = null!;

    public DateTime InitiatedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public string? TransactionId { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Cancellation Cancellation { get; set; } = null!;

    public virtual Payment Payment { get; set; } = null!;
}
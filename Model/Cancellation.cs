namespace Ecommerce_site.Model;

public class Cancellation
{
    public int CancellationId { get; set; }

    public long OrderId { get; set; }

    public string Reason { get; set; } = null!;

    public string CancellationStatus { get; set; } = null!;

    public DateTime RequestedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public long? ProcessedBy { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual User? ProcessedByNavigation { get; set; }

    public virtual Refund? Refund { get; set; }
}
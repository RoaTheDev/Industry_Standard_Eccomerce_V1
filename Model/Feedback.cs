namespace Ecommerce_site.Model;

public class Feedback
{
    public int FeedbackId { get; set; }

    public long CustomerId { get; set; }

    public long ProductId { get; set; }

    public long OrderItemId { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual OrderItem OrderItem { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
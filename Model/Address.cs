namespace Ecommerce_site.Model;

public  class Address
{
    public long AddressId { get; set; }

    public long CustomerId { get; set; }

    public string FirstAddressLine { get; set; } = null!;

    public string? SecondAddressLine { get; set; }

    public string City { get; set; } = null!;

    public string State { get; set; } = null!;

    public string PostalCode { get; set; } = null!;

    public string Country { get; set; } = null!;

    public bool IsDefault { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<Order> OrderBillingAddresses { get; set; } = new List<Order>();

    public virtual ICollection<Order> OrderShippingAddresses { get; set; } = new List<Order>();
}

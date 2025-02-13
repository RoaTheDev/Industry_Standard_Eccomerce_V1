namespace Ecommerce_site.Model;

public  class Customer
{
    public long CustomerId { get; set; }

    public DateOnly Dob { get; set; }

    public string PhoneNumber { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual User CustomerNavigation { get; set; } = null!;

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}

namespace Ecommerce_site.Model;

public  class Role
{
    public long RoleId { get; set; }

    public string RoleName { get; set; } = null!;

    public string Description { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}

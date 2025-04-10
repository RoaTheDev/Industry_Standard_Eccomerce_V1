﻿namespace Ecommerce_site.Model;

public  class Tag
{
    public long TagId { get; set; }

    public string TagName { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}

using System;
using System.Collections.Generic;

namespace MyStore.Models;

public partial class ShippingCompany
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? ContactNumber { get; set; }

    public string? Website { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}

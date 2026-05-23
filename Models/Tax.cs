using System;
using System.Collections.Generic;

namespace MyStore.Models;

public partial class Tax
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public decimal Rate { get; set; }

    public bool Status { get; set; }

    public string? Description { get; set; }

    public DateOnly? EffectiveDate { get; set; }

    public DateOnly? ExpirationDate { get; set; }

    public string? Type { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}

using System;
using System.Collections.Generic;

namespace MyStore.Models;

public partial class Product
{
    public int Id { get; set; }

    public int CategoryId { get; set; }

    public string Title { get; set; } = null!;

    public string? Filename { get; set; }

    public string? Description { get; set; }

    public bool? Deleted { get; set; }

    public int? SupplierId { get; set; }

    public int? TaxId { get; set; }

    public string? Color { get; set; }

    public string? WarrantyPeriod { get; set; }

    public string? CountryOfOrigin { get; set; }

    public DateOnly? ManufactureDate { get; set; }

    public string? Material { get; set; }

    public string? Weight { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<Gallery> Galleries { get; set; } = new List<Gallery>();

    public virtual ICollection<ProductPrice> ProductPrices { get; set; } = new List<ProductPrice>();

    public virtual Supplier? Supplier { get; set; }

    public virtual Tax? Tax { get; set; }
}

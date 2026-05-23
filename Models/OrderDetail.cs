using System;
using System.Collections.Generic;

namespace MyStore.Models;

public partial class OrderDetail
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int ProductPriceId { get; set; }

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public decimal? Discount { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual ProductPrice ProductPrice { get; set; } = null!;
}

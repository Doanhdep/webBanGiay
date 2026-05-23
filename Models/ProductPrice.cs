using System;
using System.Collections.Generic;

namespace MyStore.Models;

public partial class ProductPrice
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public int SizeId { get; set; }

    public decimal? Price { get; set; }

    public decimal? Discount { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? Deleted { get; set; }

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual Product Product { get; set; } = null!;

    public virtual ICollection<ProductVoucher> ProductVouchers { get; set; } = new List<ProductVoucher>();

    public virtual Size Size { get; set; } = null!;
}

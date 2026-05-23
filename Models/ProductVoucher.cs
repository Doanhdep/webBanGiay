using System;
using System.Collections.Generic;

namespace MyStore.Models;

public partial class ProductVoucher
{
    public int Id { get; set; }

    public int ProductPriceId { get; set; }

    public int VoucherId { get; set; }

    public decimal Discount { get; set; }

    public DateTime ValidFrom { get; set; }

    public DateTime ValidTo { get; set; }

    public int? AppliedQuantity { get; set; }

    public decimal? MaxDiscount { get; set; }

    public virtual ProductPrice ProductPrice { get; set; } = null!;

    public virtual Voucher Voucher { get; set; } = null!;
}

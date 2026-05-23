using System;
using System.Collections.Generic;

namespace MyStore.Models;

public partial class Voucher
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Code { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<ProductVoucher> ProductVouchers { get; set; } = new List<ProductVoucher>();
}

using System;
using System.Collections.Generic;

namespace MyStore.Models;

public partial class Order
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public byte? Status { get; set; }

    public int? ShippingAddressId { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public int? ShippingCompanyId { get; set; }

    public int? VoucherId { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<OrderStatus> OrderStatuses { get; set; } = new List<OrderStatus>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual Address? ShippingAddress { get; set; }

    public virtual ShippingCompany? ShippingCompany { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual Voucher? Voucher { get; set; }
}

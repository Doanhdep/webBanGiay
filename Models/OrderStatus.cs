using System;
using System.Collections.Generic;

namespace MyStore.Models;

public partial class OrderStatus
{
    public int StatusId { get; set; }

    public int OrderId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? UpdatedAt { get; set; }

    public virtual Order Order { get; set; } = null!;
}

using System;
using System.Collections.Generic;

namespace MyStore.Models;

public partial class Token
{
    public int UserId { get; set; }

    public string Token1 { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}

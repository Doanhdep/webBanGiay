using System;
using System.Collections.Generic;

namespace MyStore.Models;

public partial class Gallery
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public string Filename { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}

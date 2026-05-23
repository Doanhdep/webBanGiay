using System;
using System.Collections.Generic;

namespace MyStore.Models;

public partial class Feedback
{
    public int Id { get; set; }

    public string Note { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int UserId { get; set; }

    public int ProductPriceId { get; set; }

    public int? ParentId { get; set; }

    public int? Star { get; set; }

    public virtual ICollection<FeedbackImage> FeedbackImages { get; set; } = new List<FeedbackImage>();

    public virtual ProductPrice ProductPrice { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}

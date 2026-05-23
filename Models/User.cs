using System;
using System.Collections.Generic;

namespace MyStore.Models;

public partial class User
{
    public int Id { get; set; }

    public string Email { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string Password { get; set; } = null!;

    public int RoleId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? Deleted { get; set; }

    public string? OtpCode { get; set; }

    public bool Active { get; set; }

    public string? Filename { get; set; }

    public string Username { get; set; } = null!;

    public string? RandomKey { get; set; }

    public string? Fullname { get; set; }

    public DateTime? OtpExpiry { get; set; }

    public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<Token> Tokens { get; set; } = new List<Token>();
}

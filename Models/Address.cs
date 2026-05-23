using System;
using System.Collections.Generic;

namespace MyStore.Models;

public partial class Address
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string AddressLine { get; set; } = null!;

    public string City { get; set; } = null!;

    public string? Ward { get; set; }

    public string? District { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? FullName { get; set; }

    public string? PhoneNumber { get; set; }

    public bool IsDefault { get; set; }

    public int? CityId { get; set; }

    public int? DistrictId { get; set; }

    public int? WardId { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual User User { get; set; } = null!;
}

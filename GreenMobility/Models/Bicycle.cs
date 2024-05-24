using System;
using System.Collections.Generic;

namespace GreenMobility.Models;

public partial class Bicycle
{
    public int BicycleId { get; set; }

    public string BicycleName { get; set; } = null!;

    public string? Description { get; set; }

    public int? ParkingId { get; set; }

    public string LicensePlate { get; set; } = null!;

    public string? Photo { get; set; }

    public int? BicycleStatusId { get; set; }

    public string? Alias { get; set; }

    public DateTime? DateCreated { get; set; }

    public DateTime? DateModified { get; set; }

    public double? RentalPrice { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual BicycleStatus? BicycleStatus { get; set; }

    public virtual Parking? Parking { get; set; }

    public virtual ICollection<RentalDetail> RentalDetails { get; set; } = new List<RentalDetail>();
}

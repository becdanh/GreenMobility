using System;
using System.Collections.Generic;

namespace GreenMobility.Models;

public partial class RentalDetail
{
    public int RentalDetailId { get; set; }

    public int RentalId { get; set; }

    public int BicycleId { get; set; }

    public DateTime? PickupTime { get; set; }

    public int? RentalHours { get; set; }

    public double? RentalPrice { get; set; }

    public double? TotalMoney { get; set; }

    public virtual Bicycle Bicycle { get; set; } = null!;

    public virtual Rental Rental { get; set; } = null!;
}

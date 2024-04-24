using System;
using System.Collections.Generic;

namespace GreenMobility.Models;

public partial class RentalDetail
{
    public int RentalDetailId { get; set; }

    public int RentalId { get; set; }

    public int BicycleId { get; set; }

    public DateTime? AppointmentTime { get; set; }

    public int? HoursRented { get; set; }

    public double? RentalPrice { get; set; }

    public double? RentalFee { get; set; }

    public virtual Bicycle Bicycle { get; set; } = null!;

    public virtual Rental Rental { get; set; } = null!;
}

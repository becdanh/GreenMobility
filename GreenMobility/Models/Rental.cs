﻿using System;
using System.Collections.Generic;

namespace GreenMobility.Models;

public partial class Rental
{
    public int RentalId { get; set; }

    public int? CustomerId { get; set; }

    public DateTime OrderTime { get; set; }

    public int? EmployeeId { get; set; }

    public double? TotalMoney { get; set; }

    public int? RentalStatusId { get; set; }

    public decimal? Surcharge { get; set; }

    public string? Note { get; set; }

    public int? PickupParking { get; set; }

    public int? ReturnParking { get; set; }

    public DateTime? PickupTime { get; set; }

    public int? RentalHours { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual Employee? Employee { get; set; }

    public virtual Parking? PickupParkingNavigation { get; set; }

    public virtual ICollection<RentalDetail> RentalDetails { get; set; } = new List<RentalDetail>();

    public virtual RentalStatus? RentalStatus { get; set; }

    public virtual Parking? ReturnParkingNavigation { get; set; }
}

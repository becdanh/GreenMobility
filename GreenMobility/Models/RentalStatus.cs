using System;
using System.Collections.Generic;

namespace GreenMobility.Models;

public partial class RentalStatus
{
    public int RentalStatusId { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<Rental> Rentals { get; set; } = new List<Rental>();
}

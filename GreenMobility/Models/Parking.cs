using System;
using System.Collections.Generic;

namespace GreenMobility.Models;

public partial class Parking
{
    public int ParkingId { get; set; }

    public string ParkingName { get; set; } = null!;

    public string? Alias { get; set; }

    public string Address { get; set; } = null!;

    public bool IsActive { get; set; }

    public string? Photo { get; set; }

    public virtual ICollection<Bicycle> Bicycles { get; set; } = new List<Bicycle>();

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();

    public virtual ICollection<Rental> RentalPickupParkingNavigations { get; set; } = new List<Rental>();

    public virtual ICollection<Rental> RentalReturnParkingNavigations { get; set; } = new List<Rental>();
}

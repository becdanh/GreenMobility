using System;
using System.Collections.Generic;

namespace GreenMobility.Models
{
    public partial class Parking
    {
        public Parking()
        {
            Bicycles = new HashSet<Bicycle>();
            Employees = new HashSet<Employee>();
            RentalPickupParkingNavigations = new HashSet<Rental>();
            RentalReturnParkingNavigations = new HashSet<Rental>();
        }

        public int ParkingId { get; set; }
        public string ParkingName { get; set; } = null!;
        public string? Alias { get; set; }
        public string Address { get; set; } = null!;
        public bool IsActive { get; set; }
        public string? Photo { get; set; }

        public virtual ICollection<Bicycle> Bicycles { get; set; }
        public virtual ICollection<Employee> Employees { get; set; }
        public virtual ICollection<Rental> RentalPickupParkingNavigations { get; set; }
        public virtual ICollection<Rental> RentalReturnParkingNavigations { get; set; }
    }
}

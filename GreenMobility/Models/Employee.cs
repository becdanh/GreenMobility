﻿using System;
using System.Collections.Generic;

namespace GreenMobility.Models
{
    public partial class Employee
    {
        public Employee()
        {
            Rentals = new HashSet<Rental>();
        }

        public int EmployeeId { get; set; }
        public string FullName { get; set; } = null!;
        public DateTime? BirthDate { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public int? ParkingId { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Photo { get; set; }
        public bool? IsWorking { get; set; }

        public virtual Parking? Parking { get; set; }
        public virtual ICollection<Rental> Rentals { get; set; }
    }
}

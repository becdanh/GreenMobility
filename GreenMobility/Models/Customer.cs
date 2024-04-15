using System;
using System.Collections.Generic;

namespace GreenMobility.Models;

public partial class Customer
{
    public int CustomerId { get; set; }

    public string FullName { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public string? Password { get; set; }

    public string? Salt { get; set; }

    public DateTime? CreateDate { get; set; }

    public DateTime? LastLogin { get; set; }

    public bool IsLocked { get; set; }

    public virtual ICollection<Rental> Rentals { get; set; } = new List<Rental>();
}

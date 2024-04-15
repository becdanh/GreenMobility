using System;
using System.Collections.Generic;

namespace GreenMobility.Models;

public partial class BicycleStatus
{
    public int BicycleStatusId { get; set; }

    public string Description { get; set; } = null!;

    public virtual ICollection<Bicycle> Bicycles { get; set; } = new List<Bicycle>();
}

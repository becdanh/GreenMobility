using System;
using System.Collections.Generic;

namespace GreenMobility.Models
{
    public partial class BicycleStatus
    {
        public BicycleStatus()
        {
            Bicycles = new HashSet<Bicycle>();
        }

        public int BicycleStatusId { get; set; }
        public string Description { get; set; } = null!;

        public virtual ICollection<Bicycle> Bicycles { get; set; }
    }
}

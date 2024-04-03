using GreenMobility.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GreenMobility.ViewModels
{
    public class RentalVM
    {
        public Rental Rental { get; set; }
        public List<RentalDetail> RentalDetails { get; set; }
    }
}

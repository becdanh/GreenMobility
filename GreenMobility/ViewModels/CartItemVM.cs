using GreenMobility.Models;

namespace GreenMobility.ModelViews
{
    public class CartItemVM
    {
        public Bicycle bicycle { get; set; }

        public DateTime PickupTime { get; set; }
        public int RentalHours { get; set; }

        public double TotalMoney => RentalHours * bicycle.RentalPrice.Value;

        public int PickupParking => bicycle.ParkingId.Value;
    }
}

using GreenMobility.Models;

namespace GreenMobility.ViewModels
{
    public class CartItemVM
    {
        public Bicycle bicycle { get; set; }

        public DateTime PickupTime { get; set; }
        public int RentalHours { get; set; }

        public double TotalMoney => RentalHours * bicycle.RentalPrice.Value;

        public int PickupParking => bicycle.ParkingId.Value;

        public string ParkingName => GetParkingNameById(bicycle.ParkingId.Value);

        private string GetParkingNameById(int parkingId)
        {
            string parkingName = "Unknown";
            using (var dbContext = new GreenMobilityContext())
            {
                var parking = dbContext.Parkings.FirstOrDefault(p => p.ParkingId == parkingId);
                if (parking != null)
                {
                    parkingName = parking.ParkingName;
                }
            }
            return parkingName;
        }
    }
}

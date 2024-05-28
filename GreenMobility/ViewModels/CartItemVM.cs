using GreenMobility.Models;

namespace GreenMobility.ViewModels
{
    public class CartItemVM
    {
        public Bicycle bicycle { get; set; }

        public DateTime AppointmentTime { get; set; }
        public int RentalHours { get; set; }

        public double TotalMoney => RentalHours * bicycle.RentalPrice.Value;

        public int PickupParking => bicycle.ParkingId.Value;


        Parking parkingInfo => GetParkingInfoById(bicycle.ParkingId.Value);
        public string ParkingName => parkingInfo != null ? parkingInfo.ParkingName : "Unknown";
        public string ParkingAlias => parkingInfo != null ? parkingInfo.ParkingName : "Unknown";
        private Parking GetParkingInfoById(int parkingId)
        {
            Parking parkingInfo = new Parking();
            using (var dbContext = new GreenMobilityContext())
            {
                var parking = dbContext.Parkings.FirstOrDefault(p => p.ParkingId == parkingId);
                if (parking != null)
                {
                    parkingInfo.ParkingName = parking.ParkingName;
                    parkingInfo.Alias = parking.Alias;
                }
            }
            return parkingInfo;
        }
    }
}

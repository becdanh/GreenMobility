using AspNetCoreHero.ToastNotification.Abstractions;
using GreenMobility.Models;
using GreenMobility.ViewModels;
using Microsoft.AspNetCore.Mvc;
using GreenMobility.Extension;
using GreenMobility.Services;

namespace GreenMobility.Controllers
{
    public class RentalCartController : Controller
    {
        private readonly GreenMobilityContext _context;
        private readonly INotyfService _notyf;
        private LanguageService _localization;
        public RentalCartController(GreenMobilityContext context, INotyfService notyf, LanguageService localization)
        {
            _context = context;
            _notyf = notyf;
            _localization = localization;
        }
        public List<CartItemVM> RentalCart
        {
            get
            {
                var rentalCart = HttpContext.Session.Get<List<CartItemVM>>("RentalCart");
                if (rentalCart == default(List<CartItemVM>))
                {
                    rentalCart = new List<CartItemVM>();
                }
                return rentalCart;
            }
        }

        [HttpPost]
        [Route("api/cart/add")]
        public IActionResult AddToCart(int bicycleId)
        {
            try
            {
                List<CartItemVM> rentalCart = RentalCart;

                if (rentalCart.Any())
                {
                    int firstItemParkingId = rentalCart.First().PickupParking;

                    Bicycle newBicycle = _context.Bicycles.SingleOrDefault(p => p.BicycleId == bicycleId);
                    if (newBicycle == null)
                    {
                        return Json(new { success = false });
                    }

                    if (newBicycle.ParkingId != firstItemParkingId)
                    {
                        string errorMessage = _localization.Getkey("OnlyRent");
                        return Json(new { success = false, message = errorMessage });
                    }
                }

                if (rentalCart.Any(p => p.bicycle.BicycleId == bicycleId))
                {
                    string errorMessage = _localization.Getkey("BicycleAlready");
                    return Json(new { success = false, message = errorMessage });
                }

                Bicycle bicycle = _context.Bicycles.SingleOrDefault(p => p.BicycleId == bicycleId);
                if (bicycle == null)
                {
                    return Json(new { success = false });
                }

                CartItemVM item = new CartItemVM
                {
                    bicycle = bicycle,
                    AppointmentTime = DateTime.Now,
                };
                rentalCart.Add(item);
                HttpContext.Session.Set<List<CartItemVM>>("RentalCart", rentalCart);
                string successMessage = _localization.Getkey("SuccessAdd");
                return Json(new { success = true, message = successMessage });
            }
            catch
            {
                return Json(new { success = false });
            }
        }

        [HttpPost]
        [Route("api/cart/update")]
        public IActionResult UpdateCartInfo(int rentalHours, DateTime appointmentTime)
        {
            try
            {
                if (rentalHours == 0)
                {
                    string errorMessage = _localization.Getkey("RentalHoursInvalid");
                    return Json(new { success = false, message = errorMessage });
                }

                if (appointmentTime < DateTime.Now.AddMinutes(-1))
                {
                    string errorMessage = _localization.Getkey("InvalidAppointmentTime");
                    return Json(new { success = false, message = errorMessage });
                }

                if (appointmentTime > DateTime.Now.AddDays(2))
                {
                    string errorMessage = _localization.Getkey("InvalidAppointmentTime2");
                    return Json(new { success = false, message = errorMessage });
                }

                // Nếu thời gian hợp lệ, cập nhật thông tin giỏ hàng
                List<CartItemVM> cartItems = RentalCart;
                foreach (var item in cartItems)
                {
                    item.RentalHours = rentalHours;
                    item.AppointmentTime = appointmentTime;
                }
                // Lưu lại session
                HttpContext.Session.Set<List<CartItemVM>>("RentalCart", cartItems);
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }
        }


        [HttpGet]
        [Route("api/cart/getcartinfo")]
        public IActionResult GetCartInfo()
        {
            try
            {
                List<CartItemVM> cartItems = RentalCart;
                int rentalHours = 0;
                DateTime appointmentTime = DateTime.Now;

                if (cartItems.Any())
                {
                    rentalHours = cartItems.First().RentalHours;
                    appointmentTime = cartItems.First().AppointmentTime;
                }

                return Json(new
                {
                    success = true,
                    rentalHours = rentalHours,
                    appointmentTime = appointmentTime.ToString("yyyy-MM-ddTHH:mm")
                });
            }
            catch
            {
                return Json(new { success = false });
            }
        }


        [HttpPost]
        [Route("api/cart/remove")]
        public ActionResult Remove(int bicycleId)
        {
            try
            {
                List<CartItemVM> rentalCart = RentalCart;
                CartItemVM item = rentalCart.SingleOrDefault(p => p.bicycle.BicycleId == bicycleId);
                if (item != null)
                {
                    rentalCart.Remove(item);
                    // Lưu lại session sau khi xóa mục khỏi giỏ hàng
                    HttpContext.Session.Set<List<CartItemVM>>("RentalCart", rentalCart);
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false });
                }
            }
            catch
            {
                return Json(new { success = false });
            }
        }

        [HttpGet]
        [Route("api/cart/checkhours")]
        public IActionResult CheckRentalHours()
        {
            List<CartItemVM> cartItems = RentalCart;

            if (cartItems.Any(x => x.RentalHours == 0|| x.AppointmentTime < DateTime.Now.AddMinutes(-1) || x.AppointmentTime > DateTime.Now.AddDays(2)))
            {
                string errorMessage = _localization.Getkey("UpdateRentalHours");
                return Json(new { success = false, message = errorMessage });
            }

            return Json(new { success = true });
        }



        [Route("cart", Name = "Cart")]
        public IActionResult Index()
        {
            return View(RentalCart);
        }
    }
}
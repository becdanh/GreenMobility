using AspNetCoreHero.ToastNotification.Abstractions;
using GreenMobility.Models;
using GreenMobility.ViewModels;
using Microsoft.AspNetCore.Mvc;
using GreenMobility.Extension;

namespace GreenMobility.Controllers
{
    public class RentalCartController : Controller
    {
        private readonly GreenMobilityContext _context;
        private readonly INotyfService _notyf;
        public RentalCartController(GreenMobilityContext context, INotyfService notyf)
        {
            _context = context;
            _notyf = notyf;
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
                // Kiểm tra xem sản phẩm đã có trong giỏ hàng chưa
                if (rentalCart.Any(p => p.bicycle.BicycleId == bicycleId))
                {
                    return Json(new { success = false });
                }

                // Lấy thông tin sản phẩm từ cơ sở dữ liệu
                Bicycle hh = _context.Bicycles.SingleOrDefault(p => p.BicycleId == bicycleId);
                if (hh == null)
                {
                    return Json(new { success = false });
                }

                CartItemVM item = new CartItemVM
                {
                    bicycle = hh,
                    PickupTime = DateTime.Now,
                    RentalHours = 1
                };
                rentalCart.Add(item);
                HttpContext.Session.Set<List<CartItemVM>>("RentalCart", rentalCart);;
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }
        }

        [HttpPost]
        [Route("api/cart/update")]
        public IActionResult UpdateCartInfo(int rentalHour, DateTime pickupTime)
        {
            try
            {
                List<CartItemVM> cartItems = RentalCart;
                foreach (var item in cartItems)
                {
                    item.RentalHours = rentalHour;
                    item.PickupTime = pickupTime;
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
                int rentalHour = 0;
                DateTime pickupTime = DateTime.Now;

                if (cartItems.Any())
                {
                    rentalHour = cartItems.First().RentalHours;
                    pickupTime = cartItems.First().PickupTime;
                }

                return Json(new
                {
                    success = true,
                    rentalHour = rentalHour,
                    pickupTime = pickupTime.ToString("yyyy-MM-ddTHH:mm")
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


        [Route("cart.html", Name = "Cart")]
        public IActionResult Index()
        {
            return View(RentalCart);
        }
    }
}
using AspNetCoreHero.ToastNotification.Abstractions;
using GreenMobility.Models;
using GreenMobility.ModelViews;
using Microsoft.AspNetCore.Mvc;
using GreenMobility.Extension;

namespace GreenMobility.Controllers
{
    public class RentalCartController : Controller
    {
        private readonly GreenMobilityContext _context;

        public INotyfService _notifyService { get; }
        public RentalCartController(GreenMobilityContext context, INotyfService notifyService)
        {
            _context = context;
            _notifyService = notifyService;
        }
        // Khởi tạo giỏ hàng
        public List<CartItemVM> GioHang
        {
            get
            {
                var gh = HttpContext.Session.Get<List<CartItemVM>>("GioHang");
                if (gh == default(List<CartItemVM>))
                {
                    gh = new List<CartItemVM>();
                }
                return gh;
            }
        }

        [HttpPost]
        [Route("api/cart/add")]
        public IActionResult AddToCart(int bicycleId)
        {
            try
            {
                List<CartItemVM> gioHang = GioHang;
                // Kiểm tra xem sản phẩm đã có trong giỏ hàng chưa
                if (gioHang.Any(p => p.bicycle.BicycleId == bicycleId))
                {
                    _notifyService.Error("Sản phẩm đã tồn tại trong giỏ hàng");
                    return Json(new { success = false });
                }

                // Lấy thông tin sản phẩm từ cơ sở dữ liệu
                Bicycle hh = _context.Bicycles.SingleOrDefault(p => p.BicycleId == bicycleId);
                if (hh == null)
                {
                    _notifyService.Error("Không tìm thấy sản phẩm");
                    return Json(new { success = false });
                }

                //Thêm sản phẩm vào giỏ hàng
                CartItemVM item = new CartItemVM
                {
                    bicycle = hh,
                    PickupTime = DateTime.Now,
                    RentalHours = 1
                };
                gioHang.Add(item);//thêm vào giỏ

                //lưu lại session
                HttpContext.Session.Set<List<CartItemVM>>("GioHang", gioHang);
                _notifyService.Success("Thêm sản phẩm vào giỏ hàng thành công");
                return Json(new { success = true });
            }
            catch
            {
                _notifyService.Error("Có lỗi xảy ra khi thêm sản phẩm vào giỏ hàng");
                return Json(new { success = false });
            }
        }

        [HttpPost]
        [Route("api/cart/update")]
        public IActionResult UpdateCartInfo(int rentalHour, DateTime pickupTime)
        {
            try
            {
                List<CartItemVM> cartItems = GioHang;
                foreach (var item in cartItems)
                {
                    item.RentalHours = rentalHour;
                    item.PickupTime = pickupTime;
                }
                // Lưu lại session
                HttpContext.Session.Set<List<CartItemVM>>("GioHang", cartItems);
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
                List<CartItemVM> cartItems = GioHang;
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
                List<CartItemVM> gioHang = GioHang;
                CartItemVM item = gioHang.SingleOrDefault(p => p.bicycle.BicycleId == bicycleId);
                if (item != null)
                {
                    gioHang.Remove(item);
                    // Lưu lại session sau khi xóa mục khỏi giỏ hàng
                    HttpContext.Session.Set<List<CartItemVM>>("GioHang", gioHang);
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
            return View(GioHang);
        }
    }
}


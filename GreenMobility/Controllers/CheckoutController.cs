using AspNetCoreHero.ToastNotification.Abstractions;
using GreenMobility.Models;
using GreenMobility.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using GreenMobility.Extension;
using Microsoft.EntityFrameworkCore;
using AspNetCoreHero.ToastNotification.Notyf;
using GreenMobility.Helpper;
namespace GreenMobility.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly GreenMobilityContext _context;
        public INotyfService _notyf { get; }

        public CheckoutController(GreenMobilityContext context, INotyfService notyf)
        {
            _context = context;
            _notyf = notyf;
        }

        public List<CartItemVM> RentalCart
        {
            get
            {
                var gh = HttpContext.Session.Get<List<CartItemVM>>("RentalCart");
                if (gh == default(List<CartItemVM>))
                {
                    gh = new List<CartItemVM>();
                }
                return gh;
            }
        }

        // GET Checkout/Index
        [Route("checkout.html", Name = "Checkout")]
        public IActionResult Index(string returnUrl = null, int? parkingID = null)
        {
            // lấy giỏ hàng ra để xử lý
            var cart = HttpContext.Session.Get<List<CartItemVM>>("RentalCart");
            var taikhoanID = HttpContext.Session.GetString("CustomerId");
            CheckoutVM model = new CheckoutVM();
            if (taikhoanID != null)
            {
                var khachhang = _context.Customers.AsNoTracking().SingleOrDefault(x => x.CustomerId == Convert.ToInt32(taikhoanID));
                model.CustomerId = khachhang.CustomerId;
                model.FullName = khachhang.FullName;
                model.Email = khachhang.Email;
                model.Phone = khachhang.Phone;
            }
            if (parkingID != null)
            {
                cart = cart.Where(x => x.PickupParking == parkingID).ToList();
            }
            ViewBag.ParkingID = parkingID;
            ViewBag.GioHang = cart;
            return View(model);
        }
        [HttpPost]
        [Route("checkout.html", Name = "Checkout")]
        public IActionResult Index(int? ParkingID)
        {
            //Lay ra gio hang de xu ly
            var cart = HttpContext.Session.Get<List<CartItemVM>>("RentalCart");
            var taikhoanID = HttpContext.Session.GetString("CustomerId");
            CheckoutVM model = new CheckoutVM();
            if (taikhoanID != null)
            {
                var khachhang = _context.Customers.AsNoTracking().SingleOrDefault(x => x.CustomerId == Convert.ToInt32(taikhoanID));
                model.CustomerId = khachhang.CustomerId;
                model.FullName = khachhang.FullName;
                model.Email = khachhang.Email;
                model.Phone = khachhang.Phone;
                _context.Update(khachhang);
                _context.SaveChanges();
            }
            if (ParkingID != null)
            {
                // Lọc giỏ hàng chỉ chứa các mặt hàng thuộc ParkingID
                cart = cart.Where(x => x.PickupParking == ParkingID).ToList();
            }
            if (ModelState.IsValid)
            {
                //Khoi tao don hang
                Rental donhang = new Rental();
                donhang.CustomerId = model.CustomerId;
                donhang.OrderTime = DateTime.Now;
                donhang.RentalStatusId = 1;
                donhang.RentalFee = Convert.ToInt32(cart.Sum(x => x.TotalMoney));
                donhang.TotalMoney = Convert.ToInt32(cart.Sum(x => x.TotalMoney));
                donhang.AppointmentTime = cart.First().PickupTime;
                donhang.HoursRented = cart.First().RentalHours;
                donhang.PickupParking = cart.First().PickupParking;
                _context.Add(donhang);
                _context.SaveChanges();
                //tao danh sach don hang

                foreach (var item in cart)
                {
                    RentalDetail rentalDetail = new RentalDetail();
                    rentalDetail.RentalId = donhang.RentalId;
                    rentalDetail.BicycleId = item.bicycle.BicycleId;
                    rentalDetail.HoursRented = item.RentalHours;
                    rentalDetail.AppointmentTime = item.PickupTime;
                    rentalDetail.RentalFee = item.TotalMoney;
                    rentalDetail.RentalPrice = item.bicycle.RentalPrice;
                    _context.Add(rentalDetail);
                }
                _context.SaveChanges();
                //clear gio hang
                HttpContext.Session.Remove("RentalCart");
                //Xuat thong bao
                UpdateBicycleStatus(cart);
                _notyf.Success("Đơn hàng đặt thành công");
                //cap nhat thong tin khach hang
                return RedirectToAction("Success");
            }

            ViewBag.GioHang = cart;
            return View(model);

        }
        [Route("dat-hang-thanh-cong.html", Name = "Success")]
        public IActionResult Success()
        {
            try
            {
                var taikhoanID = HttpContext.Session.GetString("CustomerId");
                if (string.IsNullOrEmpty(taikhoanID))
                {
                    return RedirectToAction("Login", "Accounts", new { returnUrl = "/dat-hang-thanh-cong.html" });
                }
                var khachhang = _context.Customers.AsNoTracking().SingleOrDefault(x => x.CustomerId == Convert.ToInt32(taikhoanID));
                var donhang = _context.Rentals
                    .Where(x => x.CustomerId == Convert.ToInt32(taikhoanID))
                    .FirstOrDefault();
                CheckoutSuccessVM successVM = new CheckoutSuccessVM();
                successVM.FullName = khachhang.FullName;
                successVM.RentalID = donhang.RentalId;
                successVM.Phone = khachhang.Phone;
                return View(successVM);
            }
            catch
            {
                return View();
            }
        }

        private void UpdateBicycleStatus(List<CartItemVM> cart)
        {
            foreach (var item in cart)
            {
                // Lấy thông tin xe từ cơ sở dữ liệu
                var bicycle = _context.Bicycles.FirstOrDefault(x => x.BicycleId == item.bicycle.BicycleId);

                if (bicycle != null)
                {
                    // Cập nhật trạng thái của xe thành 3 (đã đặt)
                    bicycle.BicycleStatusId = 3;

                    // Cập nhật lại thông tin xe trong cơ sở dữ liệu
                    _context.Update(bicycle);
                }
            }

            // Lưu các thay đổi vào cơ sở dữ liệu
            _context.SaveChanges();
        }
    }
}

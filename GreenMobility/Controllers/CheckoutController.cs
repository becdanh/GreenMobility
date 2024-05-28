using AspNetCoreHero.ToastNotification.Abstractions;
using GreenMobility.Models;
using GreenMobility.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using GreenMobility.Extension;
using Microsoft.EntityFrameworkCore;
using AspNetCoreHero.ToastNotification.Notyf;
using GreenMobility.Helper;
using Microsoft.AspNetCore.Authorization;
using GreenMobility.Services;

namespace GreenMobility.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly GreenMobilityContext _context;
        private LanguageService _localization;
        public INotyfService _notyf { get; }

        public CheckoutController(GreenMobilityContext context, INotyfService notyf, LanguageService localization)
        {
            _context = context;
            _notyf = notyf;
            _localization = localization;
        }

        public List<CartItemVM> RentalCart
        {
            get
            {
                var cart = HttpContext.Session.Get<List<CartItemVM>>("RentalCart");
                if (cart == default(List<CartItemVM>))
                {
                    cart = new List<CartItemVM>();
                }
                return cart;
            }
        }

        [Route("checkout", Name = "Checkout")]
        public IActionResult Index()
        {
            var cart = HttpContext.Session.Get<List<CartItemVM>>("RentalCart");
            var accountId = HttpContext.Session.GetString("CustomerId");

            CheckoutVM model = new CheckoutVM();
            if (accountId != null)
            {
                var customer = _context.Customers.AsNoTracking().SingleOrDefault(x => x.CustomerId == Convert.ToInt32(accountId));
                model.CustomerId = customer.CustomerId;
                model.FullName = customer.FullName;
                model.Email = customer.Email;
                model.Phone = customer.Phone;
            }

            ViewBag.Cart = cart;
            return View(model);
        }

        [HttpPost]
        [Route("checkout", Name = "Checkout")]
        
        public IActionResult Index(CheckoutVM model)
        {
            var cart = HttpContext.Session.Get<List<CartItemVM>>("RentalCart");
            var accountId = HttpContext.Session.GetString("CustomerId");

            var bikesToRemove = new List<CartItemVM>();
            foreach (var item in cart)
            {
                var bicycle = _context.Bicycles.FirstOrDefault(x => x.BicycleId == item.bicycle.BicycleId);
                if (item.RentalHours == 0)
                {
                    _notyf.Warning(_localization.Getkey("RentalHoursInvalid"));
                    return RedirectToAction("Index", "RentalCart");
                }

                if (item.AppointmentTime < DateTime.Now.AddMinutes(-1))
                {
                    _notyf.Warning(_localization.Getkey("InvalidAppointmentTime"));
                    return RedirectToAction("Index", "RentalCart");
                }

                if (item.AppointmentTime > DateTime.Now.AddDays(2))
                {
                    _notyf.Warning(_localization.Getkey("InvalidAppointmentTime2"));
                    return RedirectToAction("Index", "RentalCart");
                }
                if (item.RentalHours == 0)
                {
                    
                    return RedirectToAction("Index", "RentalCart");
                }

                if (bicycle != null)
                {
                    if (bicycle.BicycleStatusId != 1 || bicycle.IsDeleted == true)
                    {
                        bikesToRemove.Add(item);
                    }
                }
            }

            foreach (var item in bikesToRemove)
            {
                cart.Remove(item);
            }

            if (bikesToRemove.Count > 0)
            {
                HttpContext.Session.Set("RentalCart", cart);
                _notyf.Warning(_localization.Getkey("RemoveBicycle"));
                return RedirectToAction("Index", "RentalCart");
            }

            if (ModelState.IsValid)
            {
                Rental order = new Rental();
                order.CustomerId = model.CustomerId;
                order.OrderTime = DateTime.Now;
                order.RentalStatusId = 1;
                order.RentalFee = Convert.ToInt32(cart.Sum(x => x.TotalMoney));
                order.TotalMoney = Convert.ToInt32(cart.Sum(x => x.TotalMoney));
                order.AppointmentTime = cart.First().AppointmentTime;
                order.HoursRented = cart.First().RentalHours;
                order.PickupParking = cart.First().PickupParking;
                _context.Add(order);
                _context.SaveChanges();

                foreach (var item in cart)
                {
                    RentalDetail rentalDetail = new RentalDetail();
                    rentalDetail.RentalId = order.RentalId;
                    rentalDetail.BicycleId = item.bicycle.BicycleId;
                    rentalDetail.HoursRented = item.RentalHours;
                    rentalDetail.AppointmentTime = item.AppointmentTime;
                    rentalDetail.RentalFee = item.TotalMoney;
                    rentalDetail.RentalPrice = item.bicycle.RentalPrice;
                    _context.Add(rentalDetail);
                }
                _context.SaveChanges();
                HttpContext.Session.Remove("RentalCart");
                UpdateBicycleStatus(cart);
                _notyf.Success(_localization.Getkey("OrderPlacedSuccessfully"));
                return RedirectToAction("RentalList", "Account");
            }

            ViewBag.Cart = cart;
            return View(model);
        }

        private void UpdateBicycleStatus(List<CartItemVM> cart)
        {
            foreach (var item in cart)
            {
                var bicycle = _context.Bicycles.FirstOrDefault(x => x.BicycleId == item.bicycle.BicycleId);

                if (bicycle != null)
                {
                    bicycle.BicycleStatusId = 3;
                    _context.Update(bicycle);
                }
            }
            _context.SaveChanges();
        }
    }
}

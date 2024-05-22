using AspNetCoreHero.ToastNotification.Abstractions;
using AspNetCoreHero.ToastNotification.Notyf;
using GreenMobility.Helper;
using GreenMobility.Models;
using GreenMobility.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using GreenMobility.Extension;
using GreenMobility.Services;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GreenMobility.Controllers
{
    public class AccountController : Controller
    {
        private readonly GreenMobilityContext _context;
        private LanguageService _localization;
        public INotyfService _notyf { get; }
        public AccountController(GreenMobilityContext context, INotyfService notyf, LanguageService localization)
        {
            _context = context;
            _notyf = notyf;
            _localization = localization;
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("register", Name = "Register")]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("register", Name = "Register")]
        public async Task<IActionResult> Register(RegisterVM account)
        {
            if (EmailExists(account.Email))
                ModelState.AddModelError("Email", _localization.Getkey("EmailAlreadyExists"));

            if (PhoneExists(account.Phone))
                ModelState.AddModelError("Phone", _localization.Getkey("PhoneAlreadyExists"));

            if (ModelState.IsValid)
            {


                if (PhoneExists(account.Phone))
                {
                    ModelState.AddModelError("Phone", _localization.Getkey("PhoneAlreadyExists"));
                    return View(account);
                }

                string salt = Utilities.GetRandomKey();
                Customer customer = new Customer
                {
                    FullName = account.FullName,
                    Phone = account.Phone.Trim().ToLower(),
                    Email = account.Email.Trim().ToLower(),
                    Password = (account.Password + salt.Trim()).ToMD5(),
                    IsLocked = false,
                    Salt = salt,
                    CreateDate = DateTime.Now
                };
                try
                {
                    _context.Add(customer);
                    await _context.SaveChangesAsync();

                    HttpContext.Session.SetString("CustomerId", customer.CustomerId.ToString());
                    var accountId = HttpContext.Session.GetString("CustomerId");

                    var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name,customer.FullName),
                            new Claim("CustomerId", customer.CustomerId.ToString())
                        };
                    ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "login");
                    ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                    await HttpContext.SignInAsync(claimsPrincipal);
                    _notyf.Success(_localization.Getkey("SuccessfulRegistration"));
                    return RedirectToAction("Profile", "Account");
                }
                catch
                {
                    return RedirectToAction("Register", "Account");
                }
            }
            else
            {
                _notyf.Error(_localization.Getkey("FailedRegistration"));
                return View(account);
            }
        }
        [AllowAnonymous]
        [Route("login.html", Name = "Login")]
        public IActionResult Login(string ReturnUrl = null)
        {
            var customerId = HttpContext.Session.GetString("CustomerId");
            if (customerId != null)
                return RedirectToAction("Index", "Home");
            ViewBag.ReturnUrl = ReturnUrl;
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        [Route("login.html", Name = "Login")]
        public async Task<IActionResult> Login(LoginVM model,string ReturnUrl = null)
        {
            bool isEmail = Utilities.IsValidEmail(model.UserName);
            if (!isEmail) return View(model);

            var customer = _context.Customers.AsNoTracking().SingleOrDefault(x => x.Email.Trim() == model.UserName);

            if (customer == null) return RedirectToAction("Register");
            string pass = (model.Password + customer.Salt.Trim()).ToMD5();
            if (customer.Password != pass)
            {
                _notyf.Error("Thông tin đăng nhập chưa chính xác");
                return View(model);
            }
            //kiem tra xem account co bi disable hay khong

            if (customer.IsLocked == true)
            {
                return RedirectToAction("ThongBao", "Account");
            }

            //Luu Session MaKh
            HttpContext.Session.SetString("CustomerId", customer.CustomerId.ToString());
            var taikhoanID = HttpContext.Session.GetString("CustomerId");

            //Identity
            var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, customer.FullName),
                        new Claim("CustomerId", customer.CustomerId.ToString())
                    };
            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "login");
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            await HttpContext.SignInAsync(claimsPrincipal);
            _notyf.Success("Đăng nhập thành công");

            if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
            {
                return Redirect(ReturnUrl);
            }

            return RedirectToAction("Index", "Home");
        }
        [HttpGet]
        [Route("logout.html", Name = "Logout")]
        public IActionResult Logout()
        {       
            HttpContext.Session.Clear();
            HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Route("/profile", Name = "Profile")]
        [Authorize]
        public IActionResult Profile()
        {
            var customerId = HttpContext.Session.GetString("CustomerId");
            var customer = _context.Customers.AsNoTracking().SingleOrDefault(x => x.CustomerId == Convert.ToInt32(customerId));

            return View(customer);
        }

        [HttpPost]
        [Route("/profile", Name = "Profile")]
        [Authorize]
        public async Task<IActionResult> Profile(Customer model)
        {
            try
            {
                var accountId = HttpContext.Session.GetString("CustomerId");

                if (string.IsNullOrWhiteSpace(model.FullName))
                    ModelState.AddModelError("FullName", _localization.Getkey("FullNameRequired"));

                if (string.IsNullOrWhiteSpace(model.Email))
                    ModelState.AddModelError("Email", _localization.Getkey("EmailRequired"));

                if (string.IsNullOrWhiteSpace(model.Phone))
                    ModelState.AddModelError("Phone", _localization.Getkey("PhoneRequired"));

                if (PhoneExistsExceptCurrent(model.Phone, Convert.ToInt32(accountId)))
                    ModelState.AddModelError("Phone", _localization.Getkey("PhoneAlreadyExists"));

                if (EmailExistsExceptCurrent(model.Email, Convert.ToInt32(accountId)))
                    ModelState.AddModelError("Email", _localization.Getkey("EmailAlreadyExists"));

                if (ModelState.IsValid)
                {
                    var account = await _context.Customers.FindAsync(Convert.ToInt32(accountId));
                    if (account == null)
                    {
                        return RedirectToAction("Login", "Account");
                    }

                    account.FullName = model.FullName;
                    account.Email = model.Email;
                    account.Phone = model.Phone;

                    _context.Update(account);
                    await _context.SaveChangesAsync();

                    _notyf.Success("Cập nhật thông tin thành công");

                    return RedirectToAction("Profile", "Account");
                }
                else
                {
                    _notyf.Error("Cập nhật thông tin thất bại");
                    return View("Profile", model);
                }
            }
            catch (Exception ex)
            {
                _notyf.Error("Có lỗi xảy ra khi cập nhật thông tin: " + ex.Message);
                return RedirectToAction("Profile", "Account");
            }
        }

        [Route("/rentallist", Name = "RentalList")]
        [Authorize]
        public IActionResult RentalList()
        {
            var accountId = HttpContext.Session.GetString("CustomerId");
            if (accountId != null)
            {
                var account = _context.Customers.AsNoTracking().SingleOrDefault(x => x.CustomerId == Convert.ToInt32(accountId));
                if (account != null)
                {
                    var rentalList = _context.Rentals
                        .Include(x => x.RentalStatus)
                        .AsNoTracking()
                        .Where(x => x.CustomerId == account.CustomerId)
                        .ToList();
                    return View(rentalList);
                }

            }
            return RedirectToAction("Login");
        }

        [HttpGet]
        [Route("change-password", Name = "ChangePassword")]
        [Authorize]
        public IActionResult ChangePassword()
        {
            var accountId = HttpContext.Session.GetString("CustomerId");
            return View();
        }

        [HttpPost]
        [Route("change-password", Name = "ChangePassword")]
        [Authorize]
        public IActionResult ChangePassword(ChangePasswordVM model)
        {
            var accountId = HttpContext.Session.GetString("CustomerId");

            var account = _context.Customers.Find(Convert.ToInt32(accountId));
            if (account == null) return RedirectToAction("Login", "Account");

            if (!string.IsNullOrEmpty(model.CurrentPassword) && (model.CurrentPassword + account.Salt.Trim()).ToMD5() != account.Password)
            {
                ModelState.AddModelError("CurrentPassword", _localization.Getkey("IncorrectCurrentPassword"));
            }

            if (!string.IsNullOrEmpty(model.NewPassword) && model.NewPassword.ToMD5() == account.Password)
            {
                ModelState.AddModelError("NewPassword", _localization.Getkey("CompareCurrentAndNewPassword"));
            }

            if (ModelState.IsValid)
            {

                string passnew = (model.NewPassword.Trim() + account.Salt.Trim()).ToMD5();
                account.Password = passnew;
                _context.Customers.Update(account);
                _context.SaveChanges();
                _notyf.Success("Thay đổi mật khẩu thành công");
                return RedirectToAction("Profile", "Account");
            }
            _notyf.Error("Thay đổi mật khẩu không thành công");
            return View();

        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CancelRental(int rentalId)
        {
            var rental = await _context.Rentals
                .Include(r => r.RentalDetails)
                .FirstOrDefaultAsync(r => r.RentalId == rentalId);

            if (rental == null)
            {
                return NotFound();
            }

            if (rental.RentalStatusId != 1 && rental.RentalStatusId != 6)
            {
                _notyf.Error("Không thể hủy đơn hàng này.");
                return RedirectToAction("RentalList", "Account");
            }

            rental.RentalStatusId = 5;

            foreach (var detail in rental.RentalDetails)
            {
                var bicycle = await _context.Bicycles.FindAsync(detail.BicycleId);
                if (bicycle != null)
                {
                    bicycle.BicycleStatusId = 1;
                    _context.Update(bicycle);
                }
            }

            await _context.SaveChangesAsync();

            _notyf.Success("Đã hủy đơn thuê thành công");
            return RedirectToAction("RentalList", "Account");
        }


        private bool EmailExists(string email)
        {
            return _context.Customers.Any(p => p.Email == email);
        }

        private bool PhoneExists(string phone)
        {
            return _context.Customers.Any(p => p.Phone == phone);
        }
        private bool EmailExistsExceptCurrent(string email, int id)
        {
            return _context.Customers.Any(p => p.Email == email && p.CustomerId != id);
        }


        private bool PhoneExistsExceptCurrent(string phone, int id)
        {
            return _context.Customers.Any(p => p.Phone == phone && p.CustomerId != id);
        }
    }
}
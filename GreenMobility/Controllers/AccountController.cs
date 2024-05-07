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
        [Route("register", Name = "DangKy")]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("register", Name = "DangKy")]
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
                    _notyf.Success("Đăng ký thành công");
                    return RedirectToAction("Profile", "Account");
                }
                catch
                {
                    return RedirectToAction("Register", "Account");
                }
            }
            else
            {
                return View(account);
            }
        }
        [AllowAnonymous]
        [Route("login.html", Name = "DangNhap")]
        public IActionResult Login(string returnUrl = null)
        {
            var taikhoanID = HttpContext.Session.GetString("CustomerId");
            if (taikhoanID != null)
            {
                return RedirectToAction("Profile", "Account");
            }
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        [Route("login.html", Name = "DangNhap")]
        public async Task<IActionResult> Login(LoginVM customer, string returnUrl)
        {
            bool isEmail = Utilities.IsValidEmail(customer.UserName);
            if (!isEmail) return View(customer);

            var khachhang = _context.Customers.AsNoTracking().SingleOrDefault(x => x.Email.Trim() == customer.UserName);

            if (khachhang == null) return RedirectToAction("Register");
            string pass = (customer.Password + khachhang.Salt.Trim()).ToMD5();
            if (khachhang.Password != pass)
            {
                _notyf.Error("Thông tin đăng nhập chưa chính xác");
                return View(customer);
            }
            //kiem tra xem account co bi disable hay khong

            if (khachhang.IsLocked == true)
            {
                return RedirectToAction("ThongBao", "Account");
            }

            //Luu Session MaKh
            HttpContext.Session.SetString("CustomerId", khachhang.CustomerId.ToString());
            var taikhoanID = HttpContext.Session.GetString("CustomerId");

            //Identity
            var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, khachhang.FullName),
                        new Claim("CustomerId", khachhang.CustomerId.ToString())
                    };
            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "login");
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            await HttpContext.SignInAsync(claimsPrincipal);
            _notyf.Success("Đăng nhập thành công");

            if (string.IsNullOrEmpty(returnUrl))
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return Redirect(returnUrl);
            }
        }
        [HttpGet]
        [Route("logout.html", Name = "DangXuat")]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync();
            HttpContext.Session.Remove("CustomerId");
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Route("/profile", Name = "Profile")]
        public IActionResult Profile()
        {
            var customerId = HttpContext.Session.GetString("CustomerId");
            if (customerId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var customer = _context.Customers.AsNoTracking().SingleOrDefault(x => x.CustomerId == Convert.ToInt32(customerId));

            return View(customer);
        }

        [HttpPost]
        [Route("/profile", Name = "Profile")]
        public async Task<IActionResult> Profile(Customer model)
        {
            try
            {
                var accountId = HttpContext.Session.GetString("CustomerId");

                if (accountId == null)
                {
                    return RedirectToAction("Login", "Account");
                }

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
        [AllowAnonymous]
        public IActionResult ChangePassword()
        {
            var accountId = HttpContext.Session.GetString("CustomerId");
            if (accountId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

        [HttpPost]
        public IActionResult ChangePassword(ChangePasswordVM model)
        {
            var accountId = HttpContext.Session.GetString("CustomerId");
            if (accountId == null)
            {
                return RedirectToAction("Login", "Account");
            }

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
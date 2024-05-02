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
        public IActionResult ValidatePhone(string Phone)
        {
            try
            {
                var khachhang = _context.Customers.AsNoTracking().SingleOrDefault(x => x.Phone.ToLower() == Phone.ToLower());
                if (khachhang != null)
                    return Json(data: "Số điện thoại : " + Phone + "đã được sử dụng");

                return Json(data: true);

            }
            catch
            {
                return Json(data: true);
            }
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ValidateEmail(string Email)
        {
            try
            {
                var khachhang = _context.Customers.AsNoTracking().SingleOrDefault(x => x.Email.ToLower() == Email.ToLower());
                if (khachhang != null)
                    return Json(data: "Email : " + Email + " đã được sử dụng");
                return Json(data: true);
            }
            catch
            {
                return Json(data: true);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("register.html", Name = "DangKy")]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("register.html", Name = "DangKy")]
        public async Task<IActionResult> Register(RegisterVM taikhoan)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string salt = Utilities.GetRandomKey();
                    Customer khachhang = new Customer
                    {
                        FullName = taikhoan.FullName,
                        Phone = taikhoan.Phone.Trim().ToLower(),
                        Email = taikhoan.Email.Trim().ToLower(),
                        Password = (taikhoan.Password + salt.Trim()).ToMD5(),
                        IsLocked = false,
                        Salt = salt,
                        CreateDate = DateTime.Now
                    };
                    try
                    {
                        _context.Add(khachhang);
                        await _context.SaveChangesAsync();
                        //Lưu Session MaKh
                        HttpContext.Session.SetString("CustomerId", khachhang.CustomerId.ToString());
                        var taikhoanID = HttpContext.Session.GetString("CustomerId");

                        //Identity
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name,khachhang.FullName),
                            new Claim("CustomerId", khachhang.CustomerId.ToString())
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
                    return View(taikhoan);
                }
            }
            catch
            {
                return View(taikhoan);
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
                _notyf.Success("Thông tin đăng nhập chưa chính xác");
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

        [HttpPost]
        public IActionResult ChangePassword(ChangePasswordVM model)
        {
            try
            {
                var taikhoanID = HttpContext.Session.GetString("CustomerId");
                if (taikhoanID == null)
                {
                    return RedirectToAction("Login", "Account");
                }
                if (ModelState.IsValid)
                {
                    var taikhoan = _context.Customers.Find(Convert.ToInt32(taikhoanID));
                    if (taikhoan == null) return RedirectToAction("Login", "Account");
                    var pass = (model.PasswordNow.Trim() + taikhoan.Salt.Trim()).ToMD5();
                    {
                        string passnew = (model.Password.Trim() + taikhoan.Salt.Trim()).ToMD5();
                        taikhoan.Password = passnew;
                        _context.Update(taikhoan);
                        _context.SaveChanges();
                        _notyf.Success("Đổi mật khẩu thành công");
                        return RedirectToAction("Profile", "Account");
                    }
                }
            }
            catch
            {
                _notyf.Success("Thay đổi mật khẩu không thành công");
                return RedirectToAction("Profile", "Account");
            }
            _notyf.Success("Thay đổi mật khẩu không thành công");
            return RedirectToAction("Profile", "Account");
        }

        private bool EmailExistsExceptCurrent(string email, int id)
        {
            return _context.Employees.Any(p => p.Email == email && p.EmployeeId != id);
        }

        private bool PhoneExistsExceptCurrent(string phone, int id)
        {
            return _context.Employees.Any(p => p.Phone == phone && p.EmployeeId != id);
        }
    }
}
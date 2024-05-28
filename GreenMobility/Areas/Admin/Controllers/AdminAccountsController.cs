using AspNetCoreHero.ToastNotification.Abstractions;
using GreenMobility.Areas.Admin.ViewModels;
using GreenMobility.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GreenMobility.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminAccountsController : Controller
    {
        private readonly GreenMobilityContext _context;
        private readonly INotyfService _notyf;

        public AdminAccountsController(GreenMobilityContext context, INotyfService notyf)
        {
            _context = context;
            _notyf = notyf;
        }
        public IActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Login(string ReturnUrl = null)
        {
            var EmployeeId = HttpContext.Session.GetString("EmployeeId");
            if (EmployeeId != null) return RedirectToAction("Index", "Home", new { Area = "Admin" });
            ViewBag.ReturnUrl = ReturnUrl;
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginVM model, string ReturnUrl = null)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Employee employee = _context.Employees
                    .Include(p => p.Role)
                    .SingleOrDefault(p => p.Email.ToLower() == model.UserName.ToLower().Trim());

                    if (employee == null)
                    {
                        ViewBag.Error = "Thông tin đăng nhập chưa chính xác";
                    }
                    string pass = (model.Password.Trim());
                    // + kh.Salt.Trim()
                    if (employee.Password.Trim() != pass)
                    {
                        ViewBag.Error = "Thông tin đăng nhập chưa chính xác";
                        return View(model);
                    }

                    if (employee.IsWorking == false)
                    {
                        return RedirectToAction("AccountLocked", "Error");
                    }

                    HttpContext.Session.SetString("EmployeeId", employee.EmployeeId.ToString());
                    var EmployeeId = HttpContext.Session.GetString("EmployeeId");

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, employee.FullName),
                        new Claim(ClaimTypes.Email, employee.Email),
                        new Claim("EmployeeId", employee.EmployeeId.ToString()),
                        new Claim("RoleId", employee.RoleId.ToString()),
                        new Claim("ParkingId", employee.ParkingId.ToString()),
                        new Claim("Photo", employee.Photo.ToString()),
                        new Claim(ClaimTypes.Role, employee.Role.RoleName)
                    };

                    ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                    await HttpContext.SignInAsync("AdminCookie", claimsPrincipal);
                    _notyf.Success("Đăng nhập thành công");

                    if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
                    {
                        return Redirect(ReturnUrl);
                    }

                    return RedirectToAction("Index", "Home", new { Area = "Admin" });
                }
            }
            catch
            {
                return RedirectToAction("Login", "AdminAccounts", new { Area = "Admin" });
            }
            return RedirectToAction("Login", "AdminAccounts", new { Area = "Admin" });
        }
        public IActionResult Logout()
        {
            try
            {
                HttpContext.Session.Clear();
                HttpContext.SignOutAsync("AdminCookie");
                return RedirectToAction("Login", "AdminAccounts", new { Area = "Admin" });
            }
            catch
            {
                return RedirectToAction("Login", "AdminAccounts", new { Area = "Admin" });
            }
        }
    }
}
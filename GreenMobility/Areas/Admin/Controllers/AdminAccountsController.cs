using AspNetCoreHero.ToastNotification.Abstractions;
using GreenMobility.Areas.Admin.ViewModels;
using GreenMobility.Models;
using Microsoft.AspNetCore.Authentication;
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
        [Route("admin-login.html", Name = "Login")]
        public IActionResult AdminLogin(string returnUrl = null)
        {
            var accountID = HttpContext.Session.GetString("EmployeeId");
            if (accountID != null) return RedirectToAction("Index", "Home", new { Area = "Admin" });
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        [Route("admin-login.html", Name = "Login")]
        public async Task<IActionResult> AdminLogin(LoginVM model, string returnUrl = null)
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

                    await _context.SaveChangesAsync();


                    var accountID = HttpContext.Session.GetString("EmployeeId");

                    HttpContext.Session.SetString("E", employee.EmployeeId.ToString());

                    //identity
                    var userClaims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, employee.FullName),
                        new Claim(ClaimTypes.Email, employee.Email),
                        new Claim("EmployeeId", employee.EmployeeId.ToString()),
                        new Claim("RoleId", employee.RoleId.ToString()),
                        new Claim(ClaimTypes.Role, employee.Role.RoleName)
                    };
                    var grandmaIdentity = new ClaimsIdentity(userClaims, "User Identity");
                    var userPrincipal = new ClaimsPrincipal(new[] { grandmaIdentity });
                    await HttpContext.SignInAsync(userPrincipal);



                    return RedirectToAction("Index", "Home", new { Area = "Admin" });
                }
            }
            catch
            {
                return RedirectToAction("AdminLogin", "AdminAccounts", new { Area = "Admin" });
            }
            return RedirectToAction("AdminLogin", "AdminAccounts", new { Area = "Admin" });
        }
        [Route("logout.html", Name = "Logout")]
        public IActionResult AdminLogout()
        {
            try
            {
                HttpContext.SignOutAsync();
                HttpContext.Session.Remove("EmployeeId");
                return RedirectToAction("AdminLogin", "AdminAccounts", new { Area = "Admin" });
            }
            catch
            {
                return RedirectToAction("AdminLogin", "Account", new { Area = "Admin" });
            }
        }
    }
}

using Microsoft.AspNetCore.Mvc;

namespace GreenMobility.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ErrorController : Controller
    {
        public IActionResult AccountLocked()
        {
            return View();
        }
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}

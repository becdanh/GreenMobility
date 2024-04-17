using AspNetCoreHero.ToastNotification.Abstractions;
using GreenMobility.Models;
using GreenMobility.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GreenMobility.Controllers
{
    [Authorize]
    public class RentalDetailsController : Controller
    {
        private readonly GreenMobilityContext _context;
        public INotyfService _notyf { get; }
        public RentalDetailsController(GreenMobilityContext context, INotyfService notyf)
        {
            _context = context;
            _notyf = notyf;
        }
        [HttpPost]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            try
            {
                var CustomerId = HttpContext.Session.GetString("CustomerId");
                if (string.IsNullOrEmpty(CustomerId)) return RedirectToAction("Login", "Accounts");
                var khachhang = _context.Customers.AsNoTracking().SingleOrDefault(x => x.CustomerId == Convert.ToInt32(CustomerId));
                if (khachhang == null) return NotFound();
                var rental = await _context.Rentals
                    .Include(x => x.RentalStatus)
                    .FirstOrDefaultAsync(m => m.RentalId == id && Convert.ToInt32(CustomerId) == m.CustomerId);
                if (rental == null) return NotFound();

                var rentalDetails = _context.RentalDetails
                    .Include(x => x.Bicycle)
                    .AsNoTracking()
                    .Where(x => x.RentalId == id)
                    .OrderBy(x => x.RentalDetailId)
                    .ToList();
                RentalVM Rental = new RentalVM();
                Rental.Rental = rental;
                Rental.RentalDetails = rentalDetails;
                return PartialView("Details", Rental);
            }
            catch
            {
                return NotFound();
            }
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using GreenMobility.Models;
using Microsoft.EntityFrameworkCore;

namespace GreenMobility.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SearchController : Controller
    {
        private readonly GreenMobilityContext _context;
        public SearchController(GreenMobilityContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult FindBicycle(string keyword)
        {
            List<Bicycle> ls = new List<Bicycle>();

            if (string.IsNullOrEmpty(keyword) || keyword.Length < 1)
            {
                // If no keyword is provided, return all bicycles
                ls = _context.Bicycles
                    .AsNoTracking()
                    .Include(a => a.Parking)
                    .Include(b => b.BicycleStatus)
                    .OrderByDescending(x => x.BicycleName)
                    .Take(10)
                    .ToList();
            }
            else
            {
                ls = _context.Bicycles
                    .AsNoTracking()
                    .Include(a => a.Parking)
                    .Include(b => b.BicycleStatus)
                    .Where(x => x.BicycleName.Contains(keyword)
                                || x.LicensePlate.Contains(keyword)
                                ||x.Parking.ParkingName.Contains(keyword))
                    .OrderByDescending(x => x.BicycleName)
                    .Take(10)
                    .ToList();
            }

            if (ls.Count == 0)
            {
                return PartialView("ListBicyclesSearchPartial", null);
            }
            else
            {
                return PartialView("ListBicyclesSearchPartial", ls);
            }
        }
    }
}

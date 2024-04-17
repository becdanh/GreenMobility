using GreenMobility.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PagedList.Core;

namespace GreenMobility.Controllers
{

    public class RentalsController : Controller
    {
        private readonly GreenMobilityContext _context;
        public RentalsController(GreenMobilityContext context)
        {
            _context = context;
        }

        [Route("rental.html", Name = "Rental")]
        public async Task<IActionResult> Index(int? page)
        {
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 8;
            var lsParkings = _context.Parkings
                .AsNoTracking()
                .OrderByDescending(x => x.ParkingName)
                .Include(p => p.Bicycles);

            PagedList<Parking> models = new PagedList<Parking>(lsParkings, pageNumber, pageSize);

            Dictionary<int, int> bicycleCounts = new Dictionary<int, int>();
            foreach (var parking in models)
            {
                int bicycleCount = parking.Bicycles.Count(x => x.BicycleStatusId == 1);
                bicycleCounts.Add(parking.ParkingId, bicycleCount);
            }

            ViewBag.CurrentPage = pageNumber;
            ViewBag.BicycleCounts = bicycleCounts;
            return View(models);
        }


        [Route("/{Alias}", Name = "ListBicycle")]
        public IActionResult List(string Alias, int page = 1)
        {
            try
            {
                var pageSize = 5;
                var parking = _context.Parkings.AsNoTracking().SingleOrDefault(x => x.Alias == Alias);
                var LsBicycles = _context.Bicycles
                    .AsNoTracking()
                    .Include(x => x.Parking)
                    .Where(x => x.ParkingId == parking.ParkingId && x.BicycleStatusId == 1)
                    .OrderByDescending(x => x.DateCreated);
                PagedList<Bicycle> models = new PagedList<Bicycle>(LsBicycles, page, pageSize);
                ViewBag.CurrentPage = page;
                ViewBag.CurrentParking = parking;
                ViewData["Parking"] = new SelectList(_context.Parkings, "ParkingId", "Address");
                return View(models);
            }

            catch
            {
                return RedirectToAction("Index", "Home");
            }
        }
    }
}

using GreenMobility.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PagedList.Core;

namespace GreenMobility.Controllers
{

    public class BicyclesController : Controller
    {
        private readonly GreenMobilityContext _context;
        public BicyclesController(GreenMobilityContext context)
        {
            _context = context;
        }

        [Route("rental.html", Name = "Rental")]
        public async Task<IActionResult> Index(int? page)
        {
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 10;
            var lsParkings = _context.Parkings
                .AsNoTracking()
                .OrderByDescending(x => x.ParkingName);

            PagedList<Parking> models = new PagedList<Parking>(lsParkings, pageNumber, pageSize);

            ViewBag.CurrentPage = pageNumber;
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
                    .Where(x => x.ParkingId == parking.ParkingId)
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

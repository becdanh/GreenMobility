using GreenMobility.Models;
using GreenMobility.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PagedList.Core;
using GreenMobility.Extension;

namespace GreenMobility.Controllers
{
    public class RentalsController : Controller
    {
        private readonly GreenMobilityContext _context;
        public RentalsController(GreenMobilityContext context)
        {
            _context = context;
        }

        public List<CartItemVM> RentalCart
        {
            get
            {
                var rentalCart = HttpContext.Session.Get<List<CartItemVM>>("RentalCart");
                if (rentalCart == default(List<CartItemVM>))
                {
                    rentalCart = new List<CartItemVM>();
                }
                return rentalCart;
            }
        }

        [Route("rentals", Name = "Rental")]
        public async Task<IActionResult> Index(int? page, string keyword = "")
        {
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 8;

            IQueryable<Parking> parkingQuery = _context.Parkings
                .AsNoTracking()
                .OrderByDescending(x => x.ParkingName)
                .Where(x => x.IsDeleted == false && x.IsActive == true)
                .Include(p => p.Bicycles);

            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.Trim().ToLower();
                parkingQuery = parkingQuery
                    .Where(x => x.ParkingName.ToLower().Contains(keyword)
                            || x.Address.ToLower().Contains(keyword));
            }
            var lsParkings = await parkingQuery.ToListAsync();

            PagedList<Parking> models = new PagedList<Parking>(lsParkings.AsQueryable(), pageNumber, pageSize);

            Dictionary<int, int> bicycleCounts = new Dictionary<int, int>();
            foreach (var parking in models)
            {
                int bicycleCount = parking.Bicycles.Count(x => x.BicycleStatusId == 1 && x.IsDeleted == false);
                bicycleCounts.Add(parking.ParkingId, bicycleCount);
            }
            ViewBag.Keyword = keyword;
            ViewBag.BicycleCounts = bicycleCounts;
            return View(models);
        }
        public IActionResult Search(string keyword = "")
        {
            var url = $"/rentals?keyword={keyword}";
            if (string.IsNullOrEmpty(keyword))
            {
                url = "/rentals";
            }
            return Json(new { status = "success", redirectUrl = url });
        }


        [Route("rentals/{Alias}-{id}", Name = "ListBicycle")]
        public IActionResult List(int id, int page = 1)
        {
            var pageSize = 12;
            var parking = _context.Parkings.AsNoTracking().SingleOrDefault(x => x.ParkingId == id);

            var availableBicycles = _context.Bicycles
                .AsNoTracking()
                .Include(x => x.Parking)
                .Where(x => x.ParkingId == parking.ParkingId && x.BicycleStatusId == 1 && x.IsDeleted == false)
                .OrderByDescending(x => x.DateCreated)
                .ToList();

            var bicyclesNotInCart = availableBicycles
                .Where(b => !RentalCart.Any(rc => rc.bicycle.BicycleId == b.BicycleId))
                .ToList();

            PagedList<Bicycle> models = new PagedList<Bicycle>(bicyclesNotInCart.AsQueryable(), page, pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.CurrentParking = parking;
            ViewData["Parking"] = new SelectList(_context.Parkings, "ParkingId", "Address");
            return View(models);
        }
    }
}

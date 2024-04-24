using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GreenMobility.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PagedList.Core;
using AspNetCoreHero.ToastNotification.Abstractions;

namespace GreenMobility.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminRentalsController : Controller
    {
        private readonly GreenMobilityContext _context;
        private readonly INotyfService _notyf;
        public AdminRentalsController(GreenMobilityContext context, INotyfService notyf)
        {
            _context = context;
            _notyf = notyf;
        }

        public async Task<IActionResult> Index(int? page, int status = 0, string keyword = "")
        {
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 20;

            IQueryable<Rental> rentalQuery = _context.Rentals
                .Include(r => r.Customer)
                .Include(o => o.RentalStatus)
                .AsNoTracking()
                .OrderBy(x => x.OrderTime);

            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.Trim().ToLower();
                rentalQuery = rentalQuery
                    .Where(x => x.Customer.FullName.Contains(keyword)
                            || x.Customer.Phone.Contains(keyword));
            }
            if (status != 0)
            {
                rentalQuery = rentalQuery.Where(x => x.RentalStatusId == status);
            }

            var lsRentals = await rentalQuery.ToListAsync();

            PagedList<Rental> models = new PagedList<Rental>(lsRentals.AsQueryable(), pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            ViewBag.CurrentStatus = status;
            ViewData["Status"] = new SelectList(_context.RentalStatuses, "RentalStatusId", "Description", status);
            ViewBag.CurrentKeyword = keyword;
            return View(models);
        }

        public IActionResult FilterAndSearch(int status = 0, string keyword = "")
        {
            var url = $"/Admin/AdminRentals?status={status}&keyword={keyword}";
            if (status == 0 && string.IsNullOrEmpty(keyword))
            {
                url = "/Admin/AdminRentals";
            }
            return Json(new { status = "success", redirectUrl = url });
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Rentals == null)
            {
                return NotFound();
            }

            var rental = await _context.Rentals
                .Include(r => r.Customer)
                .Include(r => r.Employee)
                .Include(r => r.RentalStatus)
                .FirstOrDefaultAsync(m => m.RentalId == id);
            if (rental == null)
            {
                return NotFound();
            }
            var rentalDetail = _context.RentalDetails
                .Include(x => x.Bicycle)
                .AsNoTracking()
                .Where(x => x.RentalId == rental.RentalId)
                .OrderBy(x => x.RentalDetailId)
                .ToList();
            ViewBag.rentalDetail = rentalDetail;

            return View(rental);
        }

        public IActionResult Create()
        {
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerId");
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "EmployeeId");
            ViewData["RentalStatusId"] = new SelectList(_context.RentalStatuses, "RentalStatusId", "RentalStatusId");
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RentalId,CustomerId,OrderTime,EmployeeId,AcceptTime,TotalMoney,RentalStatusId,Surcharge,Note")] Rental rental)
        {
            if (ModelState.IsValid)
            {
                _context.Add(rental);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerId", rental.CustomerId);
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "EmployeeId", rental.EmployeeId);
            ViewData["RentalStatusId"] = new SelectList(_context.RentalStatuses, "RentalStatusId", "RentalStatusId", rental.RentalStatusId);
            return View(rental);
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rental = await _context.Rentals
                .AsNoTracking()
                .Include(x => x.Customer)
                .FirstOrDefaultAsync(x => x.RentalId == id);
            if (rental == null)
            {
                return NotFound();
            }
            ViewData["Status"] = new SelectList(_context.RentalStatuses, "RentalStatusId", "Description", rental.RentalStatusId);
            return PartialView("Edit", rental);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, [Bind("RentalId,CustomerId,OrderTime,EmployeeId,PickupTime,TotalMoney,RentalStatusId,Surcharge,Note,RentalFee,HoursRented")] Rental rental)
        {
            if (id != rental.RentalId)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    var rent = await _context.Rentals
                        .Include(r => r.RentalDetails)
                        .Include(x => x.Customer)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.RentalId == id);

                    if (rent != null)
                    {

                        rent.RentalStatusId = rental.RentalStatusId;
                        rent.Surcharge = rental.Surcharge;
                        rent.Note = rental.Note;

                        var rentalFee = rent.RentalFee;
                        if (rent.RentalStatusId == 2)
                        {
                            rent.PickupTime = DateTime.Now;

                            foreach (var detail in rent.RentalDetails)
                            {
                                var bicycle = await _context.Bicycles.FirstOrDefaultAsync(b => b.BicycleId == detail.BicycleId);
                                if (bicycle != null)
                                {
                                    bicycle.BicycleStatusId = 2;
                                    _context.Update(bicycle);
                                }
                            }
                        }
                        if (rent.RentalStatusId == 3)
                        {
                            rent.ReturnTime = DateTime.Now;
                            foreach (var detail in rent.RentalDetails)
                            {
                                var bicycle = await _context.Bicycles.FirstOrDefaultAsync(b => b.BicycleId == detail.BicycleId);
                                if (bicycle != null)
                                {
                                    bicycle.BicycleStatusId = 1;
                                    _context.Update(bicycle);
                                }
                            }
                        }

                        if (rent.RentalStatusId == 4)
                        {
                            foreach (var detail in rent.RentalDetails)
                            {
                                var bicycle = await _context.Bicycles.FirstOrDefaultAsync(b => b.BicycleId == detail.BicycleId);
                                if (bicycle != null)
                                {
                                    bicycle.BicycleStatusId = 1;
                                    _context.Update(bicycle);
                                }
                            }
                        }
                        rent.TotalMoney = rentalFee + rental.Surcharge;

                        _context.Update(rent);
                        await _context.SaveChangesAsync();
                        _notyf.Success("Cập nhật trạng thái đơn hàng thành công");
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RentalExists(rental.RentalId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["Status"] = new SelectList(_context.RentalStatuses, "RentalStatusId", "Description", rental.RentalStatusId);
            return PartialView("Edit", rental);
        }

        private bool RentalExists(int id)
        {
            return (_context.Rentals?.Any(e => e.RentalId == id)).GetValueOrDefault();
        }


    }
}

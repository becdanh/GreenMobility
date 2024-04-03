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

namespace GreenMobility.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminRentalsController : Controller
    {
        private readonly GreenMobilityContext _context;

        public AdminRentalsController(GreenMobilityContext context)
        {
            _context = context;
        }

        // GET: Admin/AdminRentals
        public async Task<IActionResult> Index(int? page)
        {    
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 20;
            var Rentals = _context.Rentals
                .Include(r => r.Customer)
                .Include(o => o.RentalStatus)
                .AsNoTracking()
                .OrderBy(x => x.OrderTime);
            PagedList<Rental> models = new PagedList<Rental>(Rentals, pageNumber, pageSize);

            ViewBag.CurrentPage = pageNumber;

            return View(models);
        }

        // GET: Admin/AdminRentals/Details/5
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

        // GET: Admin/AdminRentals/Create
        public IActionResult Create()
        {
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerId");
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "EmployeeId");
            ViewData["RentalStatusId"] = new SelectList(_context.RentalStatuses, "RentalStatusId", "RentalStatusId");
            return View();
        }

        // POST: Admin/AdminRentals/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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

        // GET: Admin/AdminRentals/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Rentals == null)
            {
                return NotFound();
            }

            var rental = await _context.Rentals.FindAsync(id);
            if (rental == null)
            {
                return NotFound();
            }
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerId", rental.CustomerId);
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "EmployeeId", rental.EmployeeId);
            ViewData["RentalStatusId"] = new SelectList(_context.RentalStatuses, "RentalStatusId", "RentalStatusId", rental.RentalStatusId);
            return View(rental);
        }

        // POST: Admin/AdminRentals/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RentalId,CustomerId,OrderTime,EmployeeId,AcceptTime,TotalMoney,RentalStatusId,Surcharge,Note")] Rental rental)
        {
            if (id != rental.RentalId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(rental);
                    await _context.SaveChangesAsync();
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
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerId", rental.CustomerId);
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "EmployeeId", rental.EmployeeId);
            ViewData["RentalStatusId"] = new SelectList(_context.RentalStatuses, "RentalStatusId", "RentalStatusId", rental.RentalStatusId);
            return View(rental);
        }

        // GET: Admin/AdminRentals/Delete/5
        public async Task<IActionResult> Delete(int? id)
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

            return View(rental);
        }

        // POST: Admin/AdminRentals/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Rentals == null)
            {
                return Problem("Entity set 'GreenMobilityContext.Rentals'  is null.");
            }
            var rental = await _context.Rentals.FindAsync(id);
            if (rental != null)
            {
                _context.Rentals.Remove(rental);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RentalExists(int id)
        {
          return (_context.Rentals?.Any(e => e.RentalId == id)).GetValueOrDefault();
        }
    }
}

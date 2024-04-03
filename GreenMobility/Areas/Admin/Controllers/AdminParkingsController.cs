using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GreenMobility.Models;
using AspNetCoreHero.ToastNotification.Abstractions;
using PagedList.Core;
using GreenMobility.Helpper;

namespace GreenMobility.Areas.Admin.Controllers
{
    [Area("Admin")]

    public class AdminParkingsController : Controller
    {
        private readonly GreenMobilityContext _context;
        private readonly INotyfService _notyf;
        public AdminParkingsController(GreenMobilityContext context, INotyfService notyf)
        {
            _context = context;
            _notyf = notyf;
        }

        // GET: Admin/AdminParkings
        public async Task<IActionResult> Index(int page = 1, int IsActive = 0)
        {
            var pageNumber = page;
            var pageSize = 10;

            List<SelectListItem> lsTrangThai = new List<SelectListItem>();
            lsTrangThai.Add(new SelectListItem() { Text = "Tất cả trạng thái", Value = "0" });
            lsTrangThai.Add(new SelectListItem() { Text = "Hoạt động", Value = "1" });
            lsTrangThai.Add(new SelectListItem() { Text = "Khóa", Value = "2" });
            ViewData["lsTrangThai"] = lsTrangThai;

            List<Parking> lsParkings = new List<Parking>();

            if (IsActive == 1)
            {
                lsParkings = _context.Parkings
                    .Where(x => x.IsActive == true).ToList();
            }
            else if (IsActive == 2)
            {
                lsParkings = _context.Parkings
                    .Where(x => x.IsActive == false).ToList();
            }
            else
            {
                lsParkings = _context.Parkings.ToList();
            }
            PagedList<Parking> models = new PagedList<Parking>(lsParkings.AsQueryable(), pageNumber, pageSize);

            ViewBag.CurrentPage = pageNumber;
            ViewBag.CurrentActive = IsActive;

            return View(models);
        }
        public IActionResult Filter(int IsActive = 0)
        {
            var url = $"/Admin/AdminParkings?IsActive={IsActive}";
            if (IsActive == 0)
            {
                url = "/Admin/AdminParkings";
            }
            return Json(new { status = "success", redirectUrl = url });
        }

        // GET: Admin/AdminParkings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Parkings == null)
            {
                return NotFound();
            }

            var parking = await _context.Parkings
                .FirstOrDefaultAsync(m => m.ParkingId == id);
            if (parking == null)
            {
                return NotFound();
            }

            return View(parking);
        }

        // GET: Admin/AdminParkings/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ParkingId,ParkingName,Address,IsActive,Photo")] Parking parking, IFormFile? fPhoto)
        {
            if (ModelState.IsValid)
            {
                parking.ParkingName = Utilities.ToTitleCase(parking.ParkingName);
                {
                    if (fPhoto != null)
                    {
                        string extension = Path.GetExtension(fPhoto.FileName);
                        string image = Utilities.SEOUrl(parking.ParkingName) + extension;
                        parking.Photo = await Utilities.UploadFile(fPhoto, @"parkings", image.ToLower());
                    }
                }
                if (string.IsNullOrEmpty(parking.Photo)) parking.Photo = "default.jpg";

                parking.Alias = Utilities.SEOUrl(parking.ParkingName);
                _context.Add(parking);
                await _context.SaveChangesAsync();
                _notyf.Success("Tạo mới bãi đỗ thành công");
                return RedirectToAction(nameof(Index));
            }
            return View(parking);
        }

        // GET: Admin/AdminParkings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Parkings == null)
            {
                return NotFound();
            }

            var parking = await _context.Parkings.FindAsync(id);
            if (parking == null)
            {
                return NotFound();
            }
            return View(parking);
        }

        // POST: Admin/AdminParkings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ParkingId,ParkingName,Address,IsActive")] Parking parking, IFormFile? fPhoto)
        {
            if (id != parking.ParkingId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    parking.ParkingName = Utilities.ToTitleCase(parking.ParkingName);
                    {
                        if (fPhoto != null)
                        {
                            string extension = Path.GetExtension(fPhoto.FileName);
                            string image = Utilities.SEOUrl(parking.ParkingName) + extension;
                            parking.Photo = await Utilities.UploadFile(fPhoto, @"parkings", image.ToLower());
                        }
                    }
                    if (string.IsNullOrEmpty(parking.Photo)) parking.Photo = "default.jpg";

                    parking.Alias = Utilities.SEOUrl(parking.ParkingName);
                    _context.Update(parking);
                    await _context.SaveChangesAsync();
                    _notyf.Success("Cập nhật bãi đỗ thành công");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ParkingExists(parking.ParkingId))
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
            return View(parking);
        }

        // POST: Admin/AdminParkings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var parking = await _context.Parkings.FindAsync(id);
                if (parking == null)
                {
                    return NotFound();
                }

                _context.Parkings.Remove(parking);
                await _context.SaveChangesAsync();
                _notyf.Success("Xóa bãi đỗ thành công");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _notyf.Error("Xóa bãi đỗ thất bại");
                return RedirectToAction(nameof(Index));
            }
        }

        private bool ParkingExists(int id)
        {
            return (_context.Parkings?.Any(e => e.ParkingId == id)).GetValueOrDefault();
        }
    }
}

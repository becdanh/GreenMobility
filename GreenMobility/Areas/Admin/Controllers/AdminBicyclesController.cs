using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GreenMobility.Models;
using PagedList.Core;
using GreenMobility.Helpper;
using AspNetCoreHero.ToastNotification.Abstractions;

namespace GreenMobility.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminBicyclesController : Controller
    {
        private readonly GreenMobilityContext _context;
        private readonly INotyfService _notyf;
        public AdminBicyclesController(GreenMobilityContext context, INotyfService notyf)
        {
            _context = context;
            _notyf = notyf;
        }

        // GET: Admin/Bicycles
        public async Task<IActionResult> Index(int page = 1, int Status = 0, string keyword = "")
        {
            var pageNumber = page;
            var pageSize = 3;

            IQueryable<Bicycle> query = _context.Bicycles
                .AsNoTracking()
                .Include(x => x.Parking)
                .Include(x => x.BicycleStatus);

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(x => x.BicycleName.Contains(keyword)
                                    || x.Parking.ParkingName.Contains(keyword));
            }

            if (Status != 0)
            {
                query = query.Where(x => x.BicycleStatusId == Status);
            }

            var lsBicycles = await query.ToListAsync();

            PagedList<Bicycle> models = new PagedList<Bicycle>(lsBicycles.AsQueryable(), pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            ViewBag.CurrentStatus = Status;
            ViewData["TrangThai"] = new SelectList(_context.BicycleStatuses, "BicycleStatusId", "Description", Status);
            return View(models);
        }



        public IActionResult Filter(int Status = 0)
        {
            var url = $"/Admin/AdminBicycles?Status={Status}";
            if (Status == 0)
            {
                url = $"/Admin/AdminBicycles";
            }
            return Json(new { status = "success", redirectUrl = url });
        }

        [HttpGet]
        public IActionResult Search(string keyword)
        {
            var url = $"/Admin/AdminBicycles?keyword={keyword}";
            return Json(new { status = "success", redirectUrl = url });
        }

        // GET: Admin/Bicycles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Bicycles == null)
            {
                return NotFound();
            }

            var bicycle = await _context.Bicycles
                .Include(x => x.Parking)
                .Include(x => x.BicycleStatus)
                .FirstOrDefaultAsync(m => m.BicycleId == id);
            if (bicycle == null)
            {
                return NotFound();
            }

            return View(bicycle);
        }

        public IActionResult Create()
        {
            ViewData["lsBaiDo"] = new SelectList(_context.Parkings, "ParkingId", "ParkingName");
            ViewData["lsTrangThai"] = new SelectList(_context.BicycleStatuses, "BicycleStatusId", "Description");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BicycleId,BicycleName,Description,ParkingId,LicensePlate,Photo,BicycleStatusId")] Bicycle bicycle, IFormFile? fPhoto)
        {
            if (ModelState.IsValid)
            {
                bicycle.BicycleName = Utilities.ToTitleCase(bicycle.BicycleName);
                {
                    if (fPhoto != null)
                    {
                        string extension = Path.GetExtension(fPhoto.FileName);
                        string image = Utilities.SEOUrl(bicycle.BicycleName) + extension;
                        bicycle.Photo = await Utilities.UploadFile(fPhoto, @"bicycles", image.ToLower());
                    }
                }

                if (string.IsNullOrEmpty(bicycle.Photo)) bicycle.Photo = "default.jpg";

                bicycle.Alias = Utilities.SEOUrl(bicycle.BicycleName);
                bicycle.DateModified = DateTime.Now;
                bicycle.DateCreated = DateTime.Now;

                _context.Add(bicycle);
                await _context.SaveChangesAsync();
                _notyf.Success("Tạo mới thành công");
                return RedirectToAction(nameof(Index));
            }
            ViewData["lsBaiDo"] = new SelectList(_context.Parkings, "ParkingId", "ParkingName");
            ViewData["lsTrangThai"] = new SelectList(_context.BicycleStatuses, "BicycleStatusId", "Description");
            return View(bicycle);

        }

        // GET: Admin/Bicycles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Bicycles == null)
            {
                return NotFound();
            }

            var bicycle = await _context.Bicycles.FindAsync(id);
            if (bicycle == null)
            {
                return NotFound();
            }
            ViewData["lsBaiDo"] = new SelectList(_context.Parkings, "ParkingId", "ParkingName");
            ViewData["lsTrangThai"] = new SelectList(_context.BicycleStatuses, "BicycleStatusId", "Description");
            return View(bicycle);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BicycleId,BicycleName,Description,ParkingId,LicensePlate,Photo,BicycleStatusId")] Bicycle bicycle, IFormFile? fPhoto)
        {
            if (id != bicycle.BicycleId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    bicycle.BicycleName = Utilities.ToTitleCase(bicycle.BicycleName);
                    {
                        if (fPhoto != null)
                        {
                            string extension = Path.GetExtension(fPhoto.FileName);
                            string image = Utilities.SEOUrl(bicycle.BicycleName) + extension;
                            bicycle.Photo = await Utilities.UploadFile(fPhoto, @"bicycles", image.ToLower());
                        }
                    }
                    if (string.IsNullOrEmpty(bicycle.Photo)) bicycle.Photo = "default.jpg";

                    bicycle.Alias = Utilities.SEOUrl(bicycle.BicycleName);
                    bicycle.DateModified = DateTime.Now;

                    _context.Update(bicycle);
                    await _context.SaveChangesAsync();
                    _notyf.Success("Cập nhật thành công");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BicycleExists(bicycle.BicycleId))
                    {
                        _notyf.Warning("Có lỗi xảy ra");
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["lsBaiDo"] = new SelectList(_context.Parkings, "ParkingId", "ParkingName");
            ViewData["lsTrangThai"] = new SelectList(_context.BicycleStatuses, "BicycleStatusId", "Description");
            return View(bicycle);
        }


        // POST: Admin/Bicycles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var bicycle = await _context.Bicycles.FindAsync(id);
                if (bicycle == null)
                {
                    return NotFound();
                }

                _context.Bicycles.Remove(bicycle);
                await _context.SaveChangesAsync();
                _notyf.Success("Xóa xe đạp thành công");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _notyf.Error("Xóa xe đạp thất bại");
                return RedirectToAction(nameof(Index));
            }
        }

        private bool BicycleExists(int id)
        {
            return (_context.Bicycles?.Any(e => e.BicycleId == id)).GetValueOrDefault();
        }
    }
}

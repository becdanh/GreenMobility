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
using GreenMobility.Helper;
using Microsoft.AspNetCore.Authorization;

namespace GreenMobility.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminCookie")]
    public class AdminParkingsController : Controller
    {
        private readonly GreenMobilityContext _context;
        private readonly INotyfService _notyf;
        public AdminParkingsController(GreenMobilityContext context, INotyfService notyf)
        {
            _context = context;
            _notyf = notyf;
        }
        public async Task<IActionResult> Index(int page = 1, int isActive = 0, string keyword = "")
        {
            var pageNumber = page;
            var pageSize = 20;

            List<SelectListItem> lsStatus = new List<SelectListItem>();
            lsStatus.Add(new SelectListItem() { Text = "Tất cả trạng thái", Value = "0" });
            lsStatus.Add(new SelectListItem() { Text = "Hoạt động", Value = "1" });
            lsStatus.Add(new SelectListItem() { Text = "Khóa", Value = "2" });
            ViewData["lsStatus"] = lsStatus;

            IQueryable<Parking> parkingQuery = _context.Parkings
                .AsNoTracking()
                .Where(x => x.IsDeleted == false);

            if (isActive == 1)
            {
                parkingQuery = parkingQuery.Where(x => x.IsActive);
            }
            else if (isActive == 2)
            {
                parkingQuery = parkingQuery.Where(x => !x.IsActive);
            }

            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.Trim().ToLower();
                parkingQuery = parkingQuery.Where(x => x.ParkingName.ToLower().Contains(keyword) || x.Address.ToLower().Contains(keyword));
            }

            List<Parking> lsParkings = await parkingQuery.ToListAsync();
            PagedList<Parking> models = new PagedList<Parking>(lsParkings.AsQueryable(), pageNumber, pageSize);

            ViewBag.CurrentPage = pageNumber;
            ViewBag.CurrentActive = isActive;
            ViewBag.Keyword = keyword;

            return View(models);
        }
        public IActionResult FilterAndSearch(int isActive = 0, string keyword = "")
        {
            var url = $"/Admin/AdminParkings?isActive={isActive}&keyword={keyword}";
            if (isActive == 0 && string.IsNullOrEmpty(keyword))
            {
                url = "/Admin/AdminParkings";
            }
            return Json(new { status = "success", redirectUrl = url });
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Parkings == null)
            {
                return NotFound();
            }

            var parking = await _context.Parkings
                .FirstOrDefaultAsync(m => m.ParkingId == id && m.IsDeleted == false);
            if (parking == null)
            {
                return NotFound();
            }

            return View(parking);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ParkingId,ParkingName,Address,IsActive,Photo,IsDeleted")] Parking parking, IFormFile? fPhoto)
        {
            if (string.IsNullOrWhiteSpace(parking.ParkingName))
                ModelState.AddModelError("ParkingName", "Tên bãi đỗ không được để trống");

            if (string.IsNullOrWhiteSpace(parking.Address))
                ModelState.AddModelError("Address", "Địa chỉ không được để trống");

            if (ParkingNameExists(parking.ParkingName))
                ModelState.AddModelError("ParkingName", "Tên bãi đỗ đã tồn tại");

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
                parking.DateModified = DateTime.Now;
                parking.DateCreated = DateTime.Now;
                parking.IsDeleted = false;

                _context.Add(parking);
                await _context.SaveChangesAsync();
                _notyf.Success("Tạo mới thành công");
                return RedirectToAction(nameof(Index));
            }
            _notyf.Error("Tạo mới thất bại, vui lòng kiểm tra lại thông tin");
            return View(parking);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Parkings == null)
            {
                return NotFound();
            }
            var parking = await _context.Parkings.FirstOrDefaultAsync(p => p.ParkingId == id && p.IsDeleted == false);
            if (parking == null)
            {
                return NotFound();
            }
            return View(parking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ParkingId,ParkingName,Address,IsActive,Photo,IsDeleted")] Parking parking, IFormFile? fPhoto)
        {
            if (id != parking.ParkingId)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(parking.ParkingName))
                ModelState.AddModelError("ParkingName", "Tên bãi đỗ không được để trống");

            if (string.IsNullOrWhiteSpace(parking.Address))
                ModelState.AddModelError("Address", "Địa chỉ không được để trống");

            if (ParkingNameExistsExceptCurrent(parking.ParkingName, id))
                ModelState.AddModelError("ParkingName", "Tên bãi đỗ đã tồn tại");

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
                    parking.DateModified = DateTime.Now;

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
            _notyf.Error("Chỉnh sửa thất bại, vui lòng kiểm tra lại thông tin");
            return View(parking);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var bicycles = await _context.Bicycles.Where(b => b.ParkingId == id).ToListAsync();
                foreach (var bicycle in bicycles)
                {
                    bicycle.ParkingId = 2066;
                    bicycle.BicycleStatusId = 4;
                    _context.Update(bicycle);
                }
                await _context.SaveChangesAsync();

                var parking = await _context.Parkings.FindAsync(id);
                if (parking == null)
                {
                    return NotFound();
                }

                parking.IsDeleted = true;
                _context.Update(parking);
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

        private bool ParkingNameExists(string parkingName)
        {
            return _context.Parkings.Any(p => p.ParkingName == parkingName);
        }

        private bool ParkingNameExistsExceptCurrent(string parkingName, int id)
        {
            return _context.Parkings.Any(p => p.ParkingName == parkingName && p.ParkingId != id);
        }
    }
}

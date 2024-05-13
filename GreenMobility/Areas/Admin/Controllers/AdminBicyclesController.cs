using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GreenMobility.Models;
using PagedList.Core;
using GreenMobility.Helper;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;

namespace GreenMobility.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminCookie")]
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
        public async Task<IActionResult> Index(int page = 1, int status = 0, string keyword = "")
        {
            var pageNumber = page;
            var pageSize = 3;

            IQueryable<Bicycle> bicycleQuery = _context.Bicycles
                .AsNoTracking()
                .Include(x => x.Parking)
                .Include(x => x.BicycleStatus);

            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.Trim().ToLower();
                bicycleQuery = bicycleQuery
                    .Where(x => x.BicycleName.ToLower().Contains(keyword)
                            || x.Parking.Address.Contains(keyword)
                            || x.LicensePlate.Contains(keyword));
            }
            if (status != 0)
            {
                bicycleQuery = bicycleQuery.Where(x => x.BicycleStatusId == status);
            }

            var lsBicycles = await bicycleQuery.ToListAsync();

            PagedList<Bicycle> models = new PagedList<Bicycle>(lsBicycles.AsQueryable(), pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            ViewBag.CurrentStatus = status;
            ViewData["TrangThai"] = new SelectList(_context.BicycleStatuses, "BicycleStatusId", "Description", status);
            ViewBag.CurrentKeyword = keyword;
            return View(models);
        }

        public IActionResult FilterAndSearch(int status = 0, string keyword = "")
        {
            var url = $"/Admin/AdminBicycles?status={status}&keyword={keyword}";
            if (status == 0 && string.IsNullOrEmpty(keyword))
            {
                url = "/Admin/AdminBicycles";
            }
            return Json(new { status = "success", redirectUrl = url });
        }

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
        public async Task<IActionResult> Create([Bind("BicycleId,BicycleName,Description,ParkingId,LicensePlate,Photo,BicycleStatusId,RentalPrice")] Bicycle bicycle, IFormFile? fPhoto)
        {
            if (string.IsNullOrWhiteSpace(bicycle.BicycleName))
                ModelState.AddModelError("BicycleName", "Tên xe không được để trống");

            if (string.IsNullOrWhiteSpace(bicycle.LicensePlate))
                ModelState.AddModelError("LicensePlate", "Biển số không được để trống");

            if (bicycle.ParkingId == 0)
                ModelState.AddModelError("ParkingId", "Vui lòng chọn một bãi xe");

            if (bicycle.RentalPrice <= 0 || bicycle.RentalPrice.ToString() == "")
                ModelState.AddModelError("RentalPrice", "Vui lòng nhập giá thuê hợp lệ");

            if (bicycle.BicycleStatusId == 0)
                ModelState.AddModelError("BicycleStatusId", "Vui lòng chọn một trạng thái");

            if (LicensePlateExists(bicycle.LicensePlate))
            {
                ModelState.AddModelError("LicensePlate", "Biển số xe đã tồn tại");
                return View(bicycle);
            }
            if (ModelState.IsValid)
            {
                bicycle.BicycleName = Utilities.ToTitleCase(bicycle.BicycleName);
                bicycle.LicensePlate = Utilities.ToTitleCase(bicycle.LicensePlate);

                if (fPhoto != null)
                {
                    string extension = Path.GetExtension(fPhoto.FileName);
                    string image = Utilities.SEOUrl(bicycle.LicensePlate) + extension;
                    bicycle.Photo = await Utilities.UploadFile(fPhoto, @"bicycles", image.ToLower());
                }

                if (string.IsNullOrEmpty(bicycle.Photo)) bicycle.Photo = "default.jpg";

                bicycle.Alias = Utilities.SEOUrl(bicycle.LicensePlate);
                bicycle.DateModified = DateTime.Now;
                bicycle.DateCreated = DateTime.Now;

                _context.Add(bicycle);
                await _context.SaveChangesAsync();
                _notyf.Success("Tạo mới thành công");
                return RedirectToAction(nameof(Index));
            }
            _notyf.Error("Tạo mới thất bại, vui lòng kiểm tra lại thông tin");
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
        public async Task<IActionResult> Edit(int id, [Bind("BicycleId,BicycleName,Description,ParkingId,LicensePlate,Photo,BicycleStatusId,RentalPrice")] Bicycle bicycle, IFormFile? fPhoto)
        {
            if (id != bicycle.BicycleId)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(bicycle.BicycleName))
                ModelState.AddModelError("BicycleName", "Tên xe không được để trống");

            if (string.IsNullOrWhiteSpace(bicycle.LicensePlate))
                ModelState.AddModelError("LicensePlate", "Biển số không được để trống");

            if (bicycle.ParkingId == 0)
                ModelState.AddModelError("ParkingId", "Vui lòng chọn một bãi xe");

            if (bicycle.RentalPrice <= 0)
                ModelState.AddModelError("RentalPrice", "Vui lòng nhập giá thuê hợp lệ");


            if (bicycle.BicycleStatusId == 0)
                ModelState.AddModelError("BicycleStatusId", "Vui lòng chọn một trạng thái");

            if (LicensePlateExistsExceptCurrent(bicycle.LicensePlate, id))
                ModelState.AddModelError("LicensePlate", "Biển số xe đã tồn tại");

            if (ModelState.IsValid)
            {
                try
                {
                        bicycle.BicycleName = Utilities.ToTitleCase(bicycle.BicycleName);
                        bicycle.LicensePlate = Utilities.ToTitleCase(bicycle.LicensePlate);

                        if (fPhoto != null)
                        {
                            string extension = Path.GetExtension(fPhoto.FileName);
                            string image = Utilities.SEOUrl(bicycle.LicensePlate) + extension;
                            bicycle.Photo = await Utilities.UploadFile(fPhoto, @"bicycles", image.ToLower());
                        }

                        if (string.IsNullOrEmpty(bicycle.Photo)) bicycle.Photo = "default.jpg";

                        bicycle.Alias = Utilities.SEOUrl(bicycle.LicensePlate);
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
            _notyf.Error("Chỉnh sửa thất bại, vui lòng kiểm tra lại thông tin");
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
        private bool LicensePlateExists(string licensePlate)
        {
            return _context.Bicycles.Any(b => b.LicensePlate == licensePlate);
        }

        private bool LicensePlateExistsExceptCurrent(string licensePlate, int id)
        {
            return _context.Bicycles.Any(b => b.LicensePlate == licensePlate && b.BicycleId != id);
        }
    }
}

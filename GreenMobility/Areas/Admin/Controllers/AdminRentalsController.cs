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
using Microsoft.AspNetCore.Authorization;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Extensions.Hosting;

namespace GreenMobility.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminCookie")]
    public class AdminRentalsController : Controller
    {
        private readonly GreenMobilityContext _context;
        private readonly INotyfService _notyf;
        private readonly IWebHostEnvironment _hostEnvironment;
        public AdminRentalsController(GreenMobilityContext context, INotyfService notyf, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _notyf = notyf;
            _hostEnvironment = hostEnvironment;
        }

        public async Task<IActionResult> Index(int? page, int status = 0, string keyword = "")
        {
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 20;

            IQueryable<Rental> rentalQuery = _context.Rentals
                .Include(r => r.Customer)
                .Include(o => o.RentalStatus)
                .Include(p => p.PickupParkingNavigation)
                .AsNoTracking()
                .OrderByDescending(x => x.OrderTime);

            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.Trim().ToLower();
                rentalQuery = rentalQuery
                    .Where(x => x.Customer.FullName.Contains(keyword)
                        || x.PickupParkingNavigation.ParkingName.Contains(keyword)
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
            return View(rental);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, [Bind("RentalId,CustomerId,OrderTime,PickupEmployeeId,ReturnEmployeeId,PickupTime,TotalMoney,RentalStatusId,Surcharge,Note,RentalFee,HoursRented,ReturnParking")] Rental rental)
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
                            var employyeeIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "EmployeeId");

                            if (employyeeIdClaim != null)
                            {
                                if (int.TryParse(employyeeIdClaim.Value, out int employeeId))
                                {
                                    rent.PickupEmployeeId = employeeId;
                                }
                            }
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

                            var parkingIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "ParkingId");

                            int? parkingId = null;
                            if (parkingIdClaim != null)
                            {
                                if (int.TryParse(parkingIdClaim.Value, out int parsedParkingId))
                                {
                                    parkingId = parsedParkingId;
                                    rent.ReturnParking = parsedParkingId;
                                }
                            }

                            var employyeeIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "EmployeeId");

                            if (employyeeIdClaim != null)
                            {
                                if (int.TryParse(employyeeIdClaim.Value, out int employeeId))
                                {
                                    rent.ReturnEmployeeId = employeeId;
                                }
                            }
                            foreach (var detail in rent.RentalDetails)
                            {
                                var bicycle = await _context.Bicycles.FirstOrDefaultAsync(b => b.BicycleId == detail.BicycleId);
                                if (bicycle != null)
                                {
                                    bicycle.BicycleStatusId = 1;
                                    if (parkingId.HasValue)
                                    {
                                        bicycle.ParkingId = parkingId.Value;
                                    }
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

                        if (rent.RentalStatusId == 6)
                        {
                            var employyeeIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "EmployeeId");
                            if (employyeeIdClaim != null && int.TryParse(employyeeIdClaim.Value, out int employeeId))
                            {
                                var employee = await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeId == employeeId);
                                if (employee != null && employee.ParkingId == rent.PickupParking)
                                {
                                    rent.RentalStatusId = rental.RentalStatusId;
                                }
                                else
                                {
                                    _notyf.Error("Nhân viên không thuộc bãi đỗ xe này.");
                                    return RedirectToAction("Index");
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
            return View(rental);
        }

        public async Task<IActionResult> ExportExcel(string exportOption, DateTime? fromDate, DateTime? toDate, string month, int? year)
        {
            IQueryable<Rental> rentalsQuery = _context.Rentals
                .Include(r => r.Customer)
                .Include(r => r.PickupParkingNavigation)
                .Include(r => r.RentalStatus)
                .Include(r => r.RentalDetails)
                    .ThenInclude(rd => rd.Bicycle);

            switch (exportOption)
            {
                case "dateRange":
                    if (fromDate.HasValue)
                    {
                        rentalsQuery = rentalsQuery.Where(r => r.OrderTime >= fromDate.Value);
                    }
                    if (toDate.HasValue)
                    {
                        rentalsQuery = rentalsQuery.Where(r => r.OrderTime <= toDate.Value);
                    }
                    break;
                case "month":
                    if (!string.IsNullOrEmpty(month))
                    {
                        DateTime selectedMonth = DateTime.Parse(month);
                        rentalsQuery = rentalsQuery.Where(r => r.OrderTime.Year == selectedMonth.Year && r.OrderTime.Month == selectedMonth.Month);
                    }
                    break;
                case "year":
                    if (year.HasValue)
                    {
                        rentalsQuery = rentalsQuery.Where(r => r.OrderTime.Year == year.Value);
                    }
                    break;
            }

            var rentals = await rentalsQuery.ToListAsync();

            if (rentals == null || !rentals.Any())
            {
                _notyf.Error("Không có dữ liệu phù hợp để xuất.");
                return NotFound();
            }

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Rentals");

                worksheet.Cell(1, 1).Value = "Rental ID";
                worksheet.Cell(1, 2).Value = "Tên khách hàng";
                worksheet.Cell(1, 3).Value = "Bãi đỗ";
                worksheet.Cell(1, 4).Value = "Ngày đặt";
                worksheet.Cell(1, 5).Value = "Tổng tiền";
                worksheet.Cell(1, 6).Value = "Trạng thái";

                int currentRow = 2;
                foreach (var rental in rentals)
                {
                    worksheet.Cell(currentRow, 1).Value = rental.RentalId;
                    worksheet.Cell(currentRow, 2).Value = rental.Customer.FullName;
                    worksheet.Cell(currentRow, 3).Value = rental.PickupParkingNavigation.ParkingName;
                    worksheet.Cell(currentRow, 4).Value = rental.OrderTime;
                    worksheet.Cell(currentRow, 5).Value = rental.TotalMoney;
                    worksheet.Cell(currentRow, 6).Value = rental.RentalStatus.Description;
                    currentRow++;
                }

                string fileName = $"Export_Rentals_{DateTime.Now.Ticks}.xlsx";
                var filePath = Path.Combine(_hostEnvironment.WebRootPath, "Resources", "ExportExcel", fileName);
                workbook.SaveAs(filePath);

                string fileUrl = $"{Request.Scheme}://{Request.Host}/Resources/ExportExcel/{fileName}";
                return Json(new { success = true, message = "Xuất Excel thành công", fileUrl = fileUrl });
            }
        }

        private bool RentalExists(int id)
        {
            return (_context.Rentals?.Any(e => e.RentalId == id)).GetValueOrDefault();
        }


    }
}

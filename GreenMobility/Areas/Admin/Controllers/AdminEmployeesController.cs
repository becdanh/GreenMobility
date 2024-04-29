using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GreenMobility.Models;
using PagedList.Core;
using AspNetCoreHero.ToastNotification.Abstractions;
using GreenMobility.Helper;
using Microsoft.AspNetCore.Authorization;

namespace GreenMobility.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminEmployeesController : Controller
    {
        private readonly GreenMobilityContext _context;
        private readonly INotyfService _notyf;

        public AdminEmployeesController(GreenMobilityContext context, INotyfService notyf)
        {
            _context = context;
            _notyf = notyf;
        }

        // GET: Admin/AdminEmployees
        public async Task<IActionResult> Index(int page = 1, int status = 0, string keyword = "")
        {
            var pageNumber = page;
            var pageSize = 10;

            List<SelectListItem> lsStatus = new List<SelectListItem>();
            lsStatus.Add(new SelectListItem() { Text = "Tất cả trạng thái", Value = "0" });
            lsStatus.Add(new SelectListItem() { Text = "Hoạt động", Value = "1" });
            lsStatus.Add(new SelectListItem() { Text = "Khóa", Value = "2" });
            ViewData["lsStatus"] = lsStatus;

            IQueryable<Employee> employeeQuery = _context.Employees
                .AsNoTracking()
                .Include(x => x.Parking);

            if (status == 1)
            {
                employeeQuery = employeeQuery.Where(x => x.IsWorking);
            }
            else if (status == 2)
            {
                employeeQuery = employeeQuery.Where(x => !x.IsWorking);
            }

            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.Trim().ToLower();
                employeeQuery = employeeQuery.Where(x => x.FullName.ToLower().Contains(keyword) || x.Email.ToLower().Contains(keyword));
            }

            List<Employee> lsEmployees = await employeeQuery.ToListAsync();
            PagedList<Employee> models = new PagedList<Employee>(lsEmployees.AsQueryable(), pageNumber, pageSize);

            ViewBag.CurrentPage = pageNumber;
            ViewBag.CurrentStatus = status;
            ViewBag.Keyword = keyword;

            return View(models);
        }

        public IActionResult FilterAndSearch(int status = 0, string keyword = "")
        {
            var url = $"/Admin/AdminEmployees?status={status}&keyword={keyword}";
            if (status == 0 && string.IsNullOrEmpty(keyword))
            {
                url = "/Admin/AdminEmployees";
            }
            return Json(new { status = "success", redirectUrl = url });
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Employees == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(m => m.Parking)
                .FirstOrDefaultAsync(m => m.EmployeeId == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        public IActionResult Create()
        {
            ViewData["lsParkings"] = new SelectList(_context.Parkings, "ParkingId", "ParkingName");
            ViewData["lsRoles"] = new SelectList(_context.Roles, "RoleId", "RoleName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EmployeeId,FullName,BirthDate,Address,Phone,Email,Password,Photo,IsWorking,ParkingId,RoleId")] Employee employee, IFormFile fPhoto)
        {
            if (string.IsNullOrWhiteSpace(employee.FullName))
                ModelState.AddModelError("FullName", "Họ và tên không được để trống");

            if (string.IsNullOrWhiteSpace(employee.Email))
                ModelState.AddModelError("Email", "Email không được để trống");

            if (string.IsNullOrWhiteSpace(employee.Phone))
                ModelState.AddModelError("Phone", "Số điện thoại không được để trống");

            if (string.IsNullOrWhiteSpace(employee.BirthDate.ToString()))
                ModelState.AddModelError("BirthDate", "Ngày sinh không được để trống");

            if (string.IsNullOrWhiteSpace(employee.Password))
                ModelState.AddModelError("Password", "Mật khẩu không được để trống");

            if (employee.ParkingId == 0)
                ModelState.AddModelError("ParkingId", "Vui lòng chọn bãi đỗ làm việc");

            if (employee.RoleId == 0)
                ModelState.AddModelError("RoleId", "Vui lòng chọn quyền truy cập");

            if (PhoneExists(employee.Phone))
                ModelState.AddModelError("Phone", "Số điện thoại đã tồn tại");

            if (EmailExists(employee.Email))
                ModelState.AddModelError("Email", "Email đã tồn tại");

            if (ModelState.IsValid)
            {
                employee.FullName = Utilities.ToTitleCase(employee.FullName);
                employee.Address = Utilities.ToTitleCase(employee.Address);
                if (fPhoto != null)
                {
                    string extension = Path.GetExtension(fPhoto.FileName);
                    string image = Utilities.SEOUrl(employee.FullName) + extension;
                    employee.Photo = await Utilities.UploadFile(fPhoto, @"employees", image.ToLower());
                }

                if (string.IsNullOrEmpty(employee.Photo)) employee.Photo = "default.jpg";
                employee.DateModified = DateTime.Now;
                employee.DateCreated = DateTime.Now;
                employee.IsWorking = true;

                _context.Add(employee);
                await _context.SaveChangesAsync();
                _notyf.Success("Tạo mới thành công");

                return RedirectToAction(nameof(Index));
            }
            _notyf.Error("Tạo mới thất bại, vui lòng kiểm tra lại thông tin");
            ViewData["lsParkings"] = new SelectList(_context.Parkings, "ParkingId", "ParkingName");
            ViewData["lsRoles"] = new SelectList(_context.Roles, "RoleId", "RoleName");
            return View(employee);
        }

        // GET: Admin/AdminEmployees/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Employees == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }
            ViewData["lsParkings"] = new SelectList(_context.Parkings, "ParkingId", "ParkingName");
            ViewData["lsRoles"] = new SelectList(_context.Roles, "RoleId", "RoleName");
            return View(employee);
        }

        // POST: Admin/AdminEmployees/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EmployeeId,FullName,BirthDate,Address,Phone,Email,Password,Photo,IsWorking,ParkingId,RoleId")] Employee employee, IFormFile fPhoto)
        {
            if (id != employee.EmployeeId)
            {
                return NotFound();
            }
            if (string.IsNullOrWhiteSpace(employee.FullName))
                ModelState.AddModelError("FullName", "Họ và tên không được để trống");

            if (string.IsNullOrWhiteSpace(employee.Email))
                ModelState.AddModelError("Email", "Email không được để trống");

            if (string.IsNullOrWhiteSpace(employee.Phone))
                ModelState.AddModelError("Phone", "Số điện thoại không được để trống");

            if (string.IsNullOrWhiteSpace(employee.BirthDate.ToString()))
                ModelState.AddModelError("BirthDate", "Ngày sinh không được để trống");

            if (string.IsNullOrWhiteSpace(employee.Password))
                ModelState.AddModelError("Password", "Mật khẩu không được để trống");

            if (employee.ParkingId == 0)
                ModelState.AddModelError("ParkingId", "Vui lòng chọn bãi đỗ làm việc");

            if (employee.RoleId == 0)
                ModelState.AddModelError("RoleId", "Vui lòng chọn quyền truy cập");

            if (PhoneExistsExceptCurrent(employee.Phone, id))
                ModelState.AddModelError("Phone", "Số điện thoại đã tồn tại");

            if (EmailExistsExceptCurrent(employee.Email, id))
                ModelState.AddModelError("Email", "Email đã tồn tại");

            if (ModelState.IsValid)
            {  
                try
                {
                        employee.FullName = Utilities.ToTitleCase(employee.FullName);
                        employee.Address = Utilities.ToTitleCase(employee.Address);
                        if (fPhoto != null)
                        {
                            string extension = Path.GetExtension(fPhoto.FileName);
                            string image = Utilities.SEOUrl(employee.FullName) + extension;
                            employee.Photo = await Utilities.UploadFile(fPhoto, @"employees", image.ToLower());
                        }

                        if (string.IsNullOrEmpty(employee.Photo)) employee.Photo = "default.jpg";
                        employee.DateModified = DateTime.Now;

                        _context.Update(employee);
                        await _context.SaveChangesAsync();
                        _notyf.Success("Cập nhật thành công");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employee.EmployeeId))
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
            ViewData["lsParkings"] = new SelectList(_context.Parkings, "ParkingId", "ParkingName");
            ViewData["lsRoles"] = new SelectList(_context.Roles, "RoleId", "RoleName");
            return View(employee);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var employee = await _context.Employees.FindAsync(id);
                if (employee == null)
                {
                    return NotFound();
                }

                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();
                _notyf.Success("Xóa thành công");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _notyf.Error("Xóa thất bại");
                return RedirectToAction(nameof(Index));
            }
        }
        private bool EmployeeExists(int id)
        {
            return (_context.Employees?.Any(e => e.EmployeeId == id)).GetValueOrDefault();
        }

        private bool EmailExists(string email)
        {
            return _context.Employees.Any(b => b.Email == email);
        }

        private bool PhoneExists(string phone)
        {
            return _context.Employees.Any(b => b.Phone == phone);
        }

        private bool EmailExistsExceptCurrent(string email, int id)
        {
            return _context.Employees.Any(p => p.Email == email && p.EmployeeId != id);
        }

        private bool PhoneExistsExceptCurrent(string phone, int id)
        {
            return _context.Employees.Any(p => p.Phone == phone && p.EmployeeId != id);
        }
    }
}

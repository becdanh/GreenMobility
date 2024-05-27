﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GreenMobility.Models;
using AspNetCoreHero.ToastNotification.Abstractions;
using PagedList.Core;
using Microsoft.AspNetCore.Authorization;

namespace GreenMobility.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminCookie")]
    public class AdminCustomersController : Controller
    {
        private readonly GreenMobilityContext _context;
        private readonly INotyfService _notyf;
        public AdminCustomersController(GreenMobilityContext context, INotyfService notyf)
        {
            _context = context;
            _notyf = notyf;
        }

        public async Task<IActionResult> Index(int page = 1, int status = 0, string keyword = "")
        {
            var pageNumber = page;
            var pageSize = 20;

            List<SelectListItem> lsStatus = new List<SelectListItem>();
            lsStatus.Add(new SelectListItem() { Text = "Tất cả trạng thái", Value = "0" });
            lsStatus.Add(new SelectListItem() { Text = "Hoạt động", Value = "1" });
            lsStatus.Add(new SelectListItem() { Text = "Khóa", Value = "2" });
            ViewData["lsStatus"] = lsStatus;

            IQueryable<Customer> customerQuery = _context.Customers
                .AsNoTracking()
                .Where(x => x.IsDeleted == false);

            if (status == 2)
            {
                customerQuery = customerQuery.Where(x => x.IsLocked);
            }
            else if (status == 1)
            {
                customerQuery = customerQuery.Where(x => !x.IsLocked);
            }

            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.Trim().ToLower();
                customerQuery = customerQuery.Where(x => x.FullName.ToLower().Contains(keyword) || x.Email.ToLower().Contains(keyword));
            }

            List<Customer> lsCustomers = await customerQuery.ToListAsync();
            PagedList<Customer> models = new PagedList<Customer>(lsCustomers.AsQueryable(), pageNumber, pageSize);

            ViewBag.CurrentPage = pageNumber;
            ViewBag.CurrentStatus = status;
            ViewBag.Keyword = keyword;

            return View(models);
        }

        public IActionResult FilterAndSearch(int status = 0, string keyword = "")
        {
            var url = $"/Admin/AdminCustomers?status={status}&keyword={keyword}";
            if (status == 0 && string.IsNullOrEmpty(keyword))
            {
                url = "/Admin/AdminCustomers";
            }
            return Json(new { status = "success", redirectUrl = url });
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.CustomerId == id && m.IsDeleted == false);

            if (customer == null)
            {
                return NotFound();
            }

            var rentals = await _context.Rentals
            .AsNoTracking()
            .Where(r => r.CustomerId == id)
            .Include(r => r.RentalStatus)
            .Include(r => r.PickupParkingNavigation)
            .OrderByDescending(r => r.OrderTime)
            .ToListAsync();

            ViewBag.Rentals = rentals;

            return View(customer);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }


            var customer = await _context.Customers.FirstOrDefaultAsync(p => p.CustomerId == id && p.IsDeleted == false);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CustomerId,FullName,Phone,Email,Address,Password,Salt,CreateDate,LastLogin,IsLocked,IsDeleted")] Customer customer)
        {
            if (id != customer.CustomerId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(customer);
                    await _context.SaveChangesAsync();
                    _notyf.Success("Cập nhật khách hàng thành công");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomerExists(customer.CustomerId))
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
            return View(customer);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(id);
                if (customer == null)
                {
                    return NotFound();
                }

                customer.IsDeleted = true;
                customer.IsLocked = true;
                _context.Update(customer);
                await _context.SaveChangesAsync();

                _notyf.Success("Xóa khách hàng thành công");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _notyf.Error("Xóa khách hàng thất bại");
                return RedirectToAction(nameof(Index));
            }
        }
        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.CustomerId == id);
        }
    }
}

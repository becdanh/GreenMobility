using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GreenMobility.Models;
using Microsoft.Extensions.Hosting;
using PagedList.Core;
using AspNetCoreHero.ToastNotification.Abstractions;
using GreenMobility.Helper;
using AspNetCoreHero.ToastNotification.Notyf;
using Microsoft.AspNetCore.Authorization;


namespace GreenMobility.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class AdminPostsController : Controller
    {
        private readonly GreenMobilityContext _context;
        private readonly INotyfService _notyf;
        public AdminPostsController(GreenMobilityContext context, INotyfService notyf)
        {
            _context = context;
            _notyf = notyf;
        }

        // GET: Admin/AdminPosts
        public async Task<IActionResult> Index(int page = 1, int status = 0, string keyword = "")
        {
            var pageNumber = page;
            var pageSize = 10;

            List<SelectListItem> lsStatus = new List<SelectListItem>();
            lsStatus.Add(new SelectListItem() { Text = "Tất cả trạng thái", Value = "0" });
            lsStatus.Add(new SelectListItem() { Text = "Công khai", Value = "1" });
            lsStatus.Add(new SelectListItem() { Text = "Ẩn", Value = "2" });
            ViewData["lsStatus"] = lsStatus;

            IQueryable<Post> postQuery = _context.Posts
                .AsNoTracking();

            if (status == 1)
            {
                postQuery = postQuery.Where(x => x.Published);
            }
            else if (status == 2)
            {
                postQuery = postQuery.Where(x => !x.Published);
            }

            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.Trim().ToLower();
                postQuery = postQuery.Where(x => x.Title.ToLower().Contains(keyword) || x.Contents.ToLower().Contains(keyword));
            }

            List<Post> lsPosts = await postQuery.ToListAsync();
            PagedList<Post> models = new PagedList<Post>(lsPosts.AsQueryable(), pageNumber, pageSize);

            ViewBag.CurrentPage = pageNumber;
            ViewBag.CurrentStatus = status;
            ViewBag.Keyword = keyword;

            return View(models); ;
        }

        public IActionResult FilterAndSearch(int status = 0, string keyword = "")
        {
            var url = $"/Admin/AdminPosts?status={status}&keyword={keyword}";
            if (status == 0 && string.IsNullOrEmpty(keyword))
            {
                url = "/Admin/AdminPosts";
            }
            return Json(new { status = "success", redirectUrl = url });
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Posts == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .FirstOrDefaultAsync(m => m.PostId == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PostId,Title,Contents,Published,Alias,CreatedDate,Author,AccountId,Thumb")] Post post, Microsoft.AspNetCore.Http.IFormFile? fThumb)
        {
            if (string.IsNullOrWhiteSpace(post.Title))
                ModelState.AddModelError("Title", "Tiêu đề không được để trống");

            if (string.IsNullOrWhiteSpace(post.Contents))
                ModelState.AddModelError("Contents", "Nội dung không được để trống");

            if (ModelState.IsValid)
            {
                if (fThumb != null)
                {
                    string extension = Path.GetExtension(fThumb.FileName);
                    string image = Utilities.SEOUrl(post.Title) + extension;
                    post.Thumb = await Utilities.UploadFile(fThumb, @"posts", image.ToLower());
                }
                if (string.IsNullOrEmpty(post.Thumb)) post.Thumb = "default.jpg";
                post.Alias = Utilities.SEOUrl(post.Title);
                post.CreatedDate = DateTime.Now;

                _context.Add(post);
                await _context.SaveChangesAsync();
                _notyf.Success("Thêm mới thành công");
                return RedirectToAction(nameof(Index));
            }
            _notyf.Error("Tạo mới thất bại, vui lòng kiểm tra lại thông tin");
            return View(post);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }
            return View(post);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PostId,Title,Contents,Published,Alias,CreatedDate,Author,AccountId,Thumb")] Post post, Microsoft.AspNetCore.Http.IFormFile? fThumb)
        {
            if (id != post.PostId)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(post.Title))
                ModelState.AddModelError("Title", "Tiêu đề không được để trống");

            if (string.IsNullOrWhiteSpace(post.Contents))
                ModelState.AddModelError("Contents", "Nội dung không được để trống");

            if (ModelState.IsValid)
            {
                try
                {
                    if (fThumb != null && fThumb.Length>0)
                    {
                        string extension = Path.GetExtension(fThumb.FileName);
                        string imageName = Utilities.SEOUrl(post.Title) + extension;
                        post.Thumb = await Utilities.UploadFile(fThumb, @"posts", imageName.ToLower());
                    }

                    if (string.IsNullOrEmpty(post.Thumb)) post.Thumb = "default.jpg";
                    post.Alias = Utilities.SEOUrl(post.Title);

                    _context.Update(post);
                    await _context.SaveChangesAsync();
                    _notyf.Success("Chỉnh sửa thành công");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.PostId))
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
            return View(post);
        }

        // GET: Admin/AdminPosts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Posts == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .FirstOrDefaultAsync(m => m.PostId == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // POST: Admin/AdminPosts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Posts == null)
            {
                return Problem("Entity set 'GreenMobilityContext.Posts'  is null.");
            }
            var post = await _context.Posts.FindAsync(id);
            if (post != null)
            {
                _context.Posts.Remove(post);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PostExists(int id)
        {
          return (_context.Posts?.Any(e => e.PostId == id)).GetValueOrDefault();
        }
    }
}

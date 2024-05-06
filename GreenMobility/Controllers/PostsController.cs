using GreenMobility.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PagedList.Core;
namespace GreenMobility.Controllers
{
    public class PostsController : Controller
    {
        private readonly GreenMobilityContext _context;

        public PostsController(GreenMobilityContext context)
        {
            _context = context;
        }

        [Route("posts", Name = "Post")]
        public async Task<IActionResult> Index(int? page)
        {
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 2;
            var lsPosts = _context.Posts
                .AsNoTracking()
                .Where(x => x.Published == true)
                .OrderByDescending(x => x.PostId);
            PagedList<Post> models = new PagedList<Post>(lsPosts, pageNumber, pageSize);

            ViewBag.CurrentPage = pageNumber;
            return View(models);
        }

        [Route("/posts/{Alias}-{id}", Name = "PostsDetails")]
        public IActionResult Details(int id)
        {
            var post = _context.Posts.AsNoTracking().SingleOrDefault(x => x.PostId == id);
            if (post == null)
            {
                RedirectToAction("Index");
            }
            var lsRecentPosts = _context.Posts
                .AsNoTracking()
                .Where(x => x.Published == true && x.PostId != id)
                .Take(3)
                .OrderByDescending(x => x.CreatedDate)
                .ToList();
            ViewBag.RecentPost = lsRecentPosts;
            return View(post);
        }
    }
}

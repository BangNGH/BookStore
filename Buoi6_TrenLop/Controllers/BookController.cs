using Buoi6_TrenLop.Models;
using PagedList;
using System.Linq;
using System.Web.Mvc;

namespace Buoi6_TrenLop.Controllers
{
    public class BookController : Controller
    {
        private Model1 context = new Model1();
        public ActionResult Index(int? page)
        {

            int pageSize = 10;
            int pageIndex = page.HasValue ? page.Value : 1;
            var result = context.Books.ToList().ToPagedList(pageIndex, pageSize);
            ViewBag.CurrentPage = pageIndex;
            return View(result);
        }
        public ActionResult GetBookByCategry(int id, int? page)
        {
            int pageSize = 10;
            int pageIndex = page.HasValue ? page.Value : 1;
            var books = context.Books.Where(p => p.CategoryId == id).ToList();
            var result = books.ToPagedList(pageIndex, pageSize);
            ViewBag.CurrentPage = pageIndex;
            return View("Index", result);
        }


        public ActionResult Search(string searchString, int? page)
        {
            var context = new Model1();
            var rs = (from s in context.Books where s.Title.Contains(searchString) || s.Author.Contains(searchString) select s).ToList();
            if (rs.Count() > 0)
            {
                int pageSize = 10;
                int pageIndex = page.HasValue ? page.Value : 1;
                var result = rs.ToPagedList(pageIndex, pageSize);
                ViewBag.CurrentPage = pageIndex;
                return View("Index", result);
            }
            return HttpNotFound("NOT FOUND!!!");
        }


        public ActionResult GetCategory()
        {
            var listCategory = context.Categories.ToList();
            return PartialView(listCategory);
        }
        public ActionResult Details(int id)
        {
            var firstBook = context.Books.FirstOrDefault(p => p.Id == id);
            if (firstBook == null)
                return HttpNotFound("Không tìm thấy mã sách này!");
            return View(firstBook);
        }

    }
}
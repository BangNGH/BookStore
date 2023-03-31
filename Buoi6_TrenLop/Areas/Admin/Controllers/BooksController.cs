using Buoi6_TrenLop.Models;
using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;

namespace Buoi6_TrenLop.Areas.Admin.Controllers
{
    public class BooksController : ApiController
    {
        private Model1 db = new Model1();

        // GET: api/Books
        public IQueryable<Book> GetBooks()
        {
            return db.Books;
        }

        // GET: api/Books/5
        [ResponseType(typeof(Book))]
        public IHttpActionResult GetBook(int id)
        {
            Book book = db.Books.Find(id);
            if (book == null)
            {
                return NotFound();
            }

            return Ok(book);
        }

        // PUT: api/Books/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutBook(int id, Book book)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != book.Id)
            {
                return BadRequest();
            }

            db.Entry(book).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Books
        [ResponseType(typeof(Book))]
        public IHttpActionResult PostBook(Book book)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Books.Add(book);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = book.Id }, book);
        }

        // DELETE: api/Books/5
        [Route("api/Books/delete-book")]
        [ResponseType(typeof(Book))]
        public IHttpActionResult DeleteBook(int Id)
        {
            try
            {
                Book book = db.Books.Find(Id);
                if (book == null)
                {
                    return NotFound();
                }

                db.Books.Remove(book);
                db.SaveChanges();
                return Ok(book);
            }
            catch (Exception e)
            {
                return BadRequest("Cuốn sách này đang được đặt hàng!!!");
            }


        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool BookExists(int id)
        {
            return db.Books.Count(e => e.Id == id) > 0;
        }
        [HttpGet]
        [ResponseType(typeof(Book))]
        [Route("api/Books/search")]
        public IHttpActionResult GetSearchBooks(string keySearch)
        {
            if (string.IsNullOrEmpty(keySearch))
            {
                return Ok(db.Books.ToList());
            }
            var books = db.Books.Where(z => z.Title.ToLower().Contains(keySearch.ToLower())).Include(z => z.Category).ToList();
            return Ok(books);
        }

    }
}
using Librarie.Domain;
using library.mvc.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace library.mvc.Controllers
{
    public class LoansController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LoansController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Loans
                .Include(l => l.Book)
                .Include(l => l.Member);

            return View(await applicationDbContext.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loan = await _context.Loans
                .Include(l => l.Book)
                .Include(l => l.Member)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (loan == null)
            {
                return NotFound();
            }

            return View(loan);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["BookId"] = new SelectList(
                _context.Books.Where(b => b.IsAvailable),
                "Id",
                "Title"
            );

            ViewData["MemberId"] = new SelectList(
                _context.Members,
                "Id",
                "FullName"
            );

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,BookId,MemberId,LoanDate,DueDate,ReturnedDate")] Loan loan)
        {
            var otherActiveLoan = await _context.Loans
                .FirstOrDefaultAsync(l => l.BookId == loan.BookId && l.ReturnedDate == null);

            if (otherActiveLoan != null)
            {
                ModelState.AddModelError("", "This book is already on an active loan.");
            }

            var book = await _context.Books.FindAsync(loan.BookId);
            if (book == null)
            {
                ModelState.AddModelError("", "Selected book was not found.");
            }
            else if (!book.IsAvailable)
            {
                ModelState.AddModelError("", "This book is not available.");
            }

            if (ModelState.IsValid)
            {
                book.IsAvailable = false;

                _context.Add(loan);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["BookId"] = new SelectList(
                _context.Books.Where(b => b.IsAvailable || b.Id == loan.BookId),
                "Id",
                "Title",
                loan.BookId
            );

            ViewData["MemberId"] = new SelectList(
                _context.Members,
                "Id",
                "FullName",
                loan.MemberId
            );

            return View(loan);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loan = await _context.Loans.FindAsync(id);
            if (loan == null)
            {
                return NotFound();
            }

            ViewData["BookId"] = new SelectList(_context.Books, "Id", "Title", loan.BookId);
            ViewData["MemberId"] = new SelectList(_context.Members, "Id", "FullName", loan.MemberId);
            return View(loan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,BookId,MemberId,LoanDate,DueDate,ReturnedDate")] Loan loan)
        {
            if (id != loan.Id)
            {
                return NotFound();
            }

            var existingLoan = await _context.Loans
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == id);

            if (existingLoan == null)
            {
                return NotFound();
            }

            var otherActiveLoan = await _context.Loans
                .FirstOrDefaultAsync(l => l.BookId == loan.BookId && l.Id != loan.Id && l.ReturnedDate == null);

            if (otherActiveLoan != null && loan.ReturnedDate == null)
            {
                ModelState.AddModelError("", "This book is already on an active loan.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var book = await _context.Books.FindAsync(loan.BookId);
                    if (book != null)
                    {
                        book.IsAvailable = loan.ReturnedDate != null;
                    }

                    _context.Update(loan);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LoanExists(loan.Id))
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

            ViewData["BookId"] = new SelectList(_context.Books, "Id", "Title", loan.BookId);
            ViewData["MemberId"] = new SelectList(_context.Members, "Id", "FullName", loan.MemberId);
            return View(loan);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loan = await _context.Loans
                .Include(l => l.Book)
                .Include(l => l.Member)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (loan == null)
            {
                return NotFound();
            }

            return View(loan);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var loan = await _context.Loans.FindAsync(id);

            if (loan != null)
            {
                var book = await _context.Books.FindAsync(loan.BookId);

                if (book != null && loan.ReturnedDate == null)
                {
                    book.IsAvailable = true;
                }

                _context.Loans.Remove(loan);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> MarkReturned(int id)
        {
            var loan = await _context.Loans.FindAsync(id);

            if (loan == null)
            {
                return NotFound();
            }

            if (loan.ReturnedDate == null)
            {
                loan.ReturnedDate = DateTime.Now;

                var book = await _context.Books.FindAsync(loan.BookId);
                if (book != null)
                {
                    book.IsAvailable = true;
                }

                _context.Update(loan);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool LoanExists(int id)
        {
            return _context.Loans.Any(e => e.Id == id);
        }
    }
}
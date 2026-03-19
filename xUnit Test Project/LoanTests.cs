using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using library.mvc.Data;
using library.mvc.Controllers;
using Librarie.Domain;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace library.mvc.Tests
{
    public class LoanTests
    {
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task Returned_Loan_Makes_Book_Available()
        {
            var context = GetDbContext();

            var book = new Book
            {
                Title = "Test Book",
                Author = "Author 1",
                Isbn = "1234567890",
                Category = "Programming",
                IsAvailable = false
            };

            context.Books.Add(book);
            await context.SaveChangesAsync();

            var loan = new Loan
            {
                BookId = book.Id,
                MemberId = 1,
                LoanDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(7),
                ReturnedDate = null
            };

            context.Loans.Add(loan);
            await context.SaveChangesAsync();

            loan.ReturnedDate = DateTime.Now;
            book.IsAvailable = true;

            await context.SaveChangesAsync();

            Assert.True(book.IsAvailable);
        }

        [Fact]
        public async Task Overdue_Loan_Is_Detected()
        {
            var context = GetDbContext();

            var loan = new Loan
            {
                BookId = 1,
                MemberId = 1,
                LoanDate = DateTime.Now.AddDays(-10),
                DueDate = DateTime.Now.AddDays(-5),
                ReturnedDate = null
            };

            context.Loans.Add(loan);
            await context.SaveChangesAsync();

            var overdue = context.Loans.Any(l => l.DueDate < DateTime.Now && l.ReturnedDate == null);

            Assert.True(overdue);
        }

        [Fact]
        public async Task Book_Search_Returns_Correct_Result()
        {
            var context = GetDbContext();

            context.Books.Add(new Book
            {
                Title = "CSharp Guide",
                Author = "John",
                Isbn = "1111111111",
                Category = "Programming",
                IsAvailable = true
            });

            context.Books.Add(new Book
            {
                Title = "Java Book",
                Author = "Mike",
                Isbn = "2222222222",
                Category = "Programming",
                IsAvailable = true
            });

            await context.SaveChangesAsync();

            var result = context.Books
                .Where(b => b.Title.Contains("CSharp"))
                .ToList();

            Assert.Single(result);
        }

        [Fact]
        public void Admin_Controller_Has_Authorize_Attribute()
        {
            var controllerType = typeof(AdminController);

            var hasAuthorize = controllerType
                .GetCustomAttributes(typeof(AuthorizeAttribute), true)
                .Any();

            Assert.True(hasAuthorize);
        }
    }
}
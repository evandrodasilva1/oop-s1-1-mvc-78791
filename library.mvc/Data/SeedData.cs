using Bogus;
using Librarie.Domain;
using library.mvc.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace library.mvc.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());

            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            await context.Database.MigrateAsync();

            await SeedAdminAsync(roleManager, userManager);
            await SeedLibraryDataAsync(context);
        }

        private static async Task SeedAdminAsync(
            RoleManager<IdentityRole> roleManager,
            UserManager<IdentityUser> userManager)
        {
            const string adminRole = "Admin";
            const string adminEmail = "admin@library.com";
            const string adminPassword = "Admin123!";

            if (!await roleManager.RoleExistsAsync(adminRole))
            {
                await roleManager.CreateAsync(new IdentityRole(adminRole));
            }

            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, adminRole);
                }
            }
            else
            {
                if (!await userManager.IsInRoleAsync(adminUser, adminRole))
                {
                    await userManager.AddToRoleAsync(adminUser, adminRole);
                }
            }
        }

        private static async Task SeedLibraryDataAsync(ApplicationDbContext context)
        {
            if (context.Books.Any() || context.Members.Any() || context.Loans.Any())
            {
                return;
            }

            var categoryList = new[]
            {
                "Programming", "Database", "AI", "Networking", "Web", "Security", "Cloud", "Software"
            };

            var bookFaker = new Faker<Book>()
                .RuleFor(b => b.Title, f => f.Commerce.ProductName())
                .RuleFor(b => b.Author, f => f.Name.FullName())
                .RuleFor(b => b.Isbn, f => f.Random.ReplaceNumbers("##########"))
                .RuleFor(b => b.Category, f => f.PickRandom(categoryList))
                .RuleFor(b => b.IsAvailable, true);

            var memberFaker = new Faker<Member>()
                .RuleFor(m => m.FullName, f => f.Name.FullName())
                .RuleFor(m => m.Email, (f, m) => f.Internet.Email(m.FullName))
                .RuleFor(m => m.Phone, f => f.Phone.PhoneNumber());

            var books = bookFaker.Generate(20);
            var members = memberFaker.Generate(10);

            context.Books.AddRange(books);
            context.Members.AddRange(members);
            await context.SaveChangesAsync();

            var random = new Random();

            var selectedBooks = books.OrderBy(x => Guid.NewGuid()).Take(15).ToList();
            var loans = new List<Loan>();

            for (int i = 0; i < 15; i++)
            {
                var book = selectedBooks[i];
                var member = members[random.Next(members.Count)];

                DateTime loanDate;
                DateTime dueDate;
                DateTime? returnedDate;

                if (i < 5)
                {
                    loanDate = DateTime.Today.AddDays(-20 - i);
                    dueDate = loanDate.AddDays(7);
                    returnedDate = loanDate.AddDays(3);
                    book.IsAvailable = true;
                }
                else if (i < 10)
                {
                    loanDate = DateTime.Today.AddDays(-4 - i);
                    dueDate = loanDate.AddDays(14);
                    returnedDate = null;
                    book.IsAvailable = false;
                }
                else
                {
                    loanDate = DateTime.Today.AddDays(-25 - i);
                    dueDate = loanDate.AddDays(7);
                    returnedDate = null;
                    book.IsAvailable = false;
                }

                loans.Add(new Loan
                {
                    BookId = book.Id,
                    MemberId = member.Id,
                    LoanDate = loanDate,
                    DueDate = dueDate,
                    ReturnedDate = returnedDate
                });
            }

            context.Loans.AddRange(loans);
            await context.SaveChangesAsync();
        }
    }
}
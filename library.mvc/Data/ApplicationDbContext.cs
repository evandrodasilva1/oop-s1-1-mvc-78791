using Librarie.Domain;
using library.mvc.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace library.mvc.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Book> Books => Set<Book>();
        public DbSet<Member> Members => Set<Member>();
        public DbSet<Loan> Loans => Set<Loan>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Book>()
                .HasIndex(b => b.Isbn)
                .IsUnique();

            modelBuilder.Entity<Loan>()
                .HasOne(l => l.Book)
                .WithMany(b => b.Loans)
                .HasForeignKey(l => l.BookId);

            modelBuilder.Entity<Member>()
                .HasMany(m => m.Loans)
                .WithOne(l => l.Member)
                .HasForeignKey(l => l.MemberId);

            modelBuilder.Entity<Book>().HasData(
                new Book { Id = 1, Title = "C# Basics", Author = "John Smith", Isbn = "1111", Category = "Programming", IsAvailable = true },
                new Book { Id = 2, Title = "ASP.NET Guide", Author = "Anna Brown", Isbn = "2222", Category = "Programming", IsAvailable = true },
                new Book { Id = 3, Title = "Clean Code", Author = "Robert Martin", Isbn = "3333", Category = "Programming", IsAvailable = true },
                new Book { Id = 4, Title = "Design Patterns", Author = "Gamma", Isbn = "4444", Category = "Programming", IsAvailable = true },
                new Book { Id = 5, Title = "Database Systems", Author = "Thomas Connolly", Isbn = "5555", Category = "Database", IsAvailable = true },
                new Book { Id = 6, Title = "Operating Systems", Author = "Tanenbaum", Isbn = "6666", Category = "Systems", IsAvailable = true },
                new Book { Id = 7, Title = "Artificial Intelligence", Author = "Russell", Isbn = "7777", Category = "AI", IsAvailable = true },
                new Book { Id = 8, Title = "Machine Learning", Author = "Tom Mitchell", Isbn = "8888", Category = "AI", IsAvailable = true },
                new Book { Id = 9, Title = "Deep Learning", Author = "Ian Goodfellow", Isbn = "9999", Category = "AI", IsAvailable = true },
                new Book { Id = 10, Title = "Computer Networks", Author = "Tanenbaum", Isbn = "1010", Category = "Networking", IsAvailable = true }
            );

            modelBuilder.Entity<Member>().HasData(
                new Member { Id = 1, FullName = "Alice Johnson", Email = "alice@email.com" },
                new Member { Id = 2, FullName = "Bob Smith", Email = "bob@email.com" },
                new Member { Id = 3, FullName = "Charlie Brown", Email = "charlie@email.com" },
                new Member { Id = 4, FullName = "David Miller", Email = "david@email.com" },
                new Member { Id = 5, FullName = "Emma Wilson", Email = "emma@email.com" }
            );
        }
    }
}
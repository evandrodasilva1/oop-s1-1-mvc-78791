using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Librarie.Domain
{
    public class Book
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = "";

        [Required]
        public string Author { get; set; } = "";

        public string Isbn { get; set; } = "";

        public string Category { get; set; } = "";

        public bool IsAvailable { get; set; } = true;

        public ICollection<Loan>? Loans { get; set; }
    }
}
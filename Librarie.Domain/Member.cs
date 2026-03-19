using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Librarie.Domain
{
    public class Member
    {
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; } = "";

        public string Email { get; set; } = "";

        public string Phone { get; set; } = "";

        public ICollection<Loan>? Loans { get; set; }
    }
}
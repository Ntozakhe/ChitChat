using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChitChat.Models
{
    public class AppUser : IdentityUser
    {
        [Required]
        [Display(Name = "First Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and a Max {1} charactors", MinimumLength = 2)]
        public string? FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and a Max {1} charactors", MinimumLength = 2)]
        public string? LastName { get; set; }

        [NotMapped] //this wont persisted in the database but it can be calculated at runtime.
        public string? FullName { get { return $"{FirstName} {LastName}"; } }
    }
}

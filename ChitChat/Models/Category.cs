using System.ComponentModel.DataAnnotations;

namespace ChitChat.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Category Name")]
        public string? Name { get; set; }

        [Required]
        public string? AppUserId { get; set; }

        //Virtuals
        public virtual AppUser? AppUser { get; set; }
        //This tells our application to create a FK to The AppUser model.
        //And then the AppUSer model is our asp net identity.

        public virtual ICollection<Contact> Contacts { get; set; } = new HashSet<Contact>();

    }
}

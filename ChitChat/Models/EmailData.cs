using System.ComponentModel.DataAnnotations;

namespace ChitChat.Models
{
    public class EmailData
    {
        [Required]
        public string EmailAddress { get; set; } = "";

        [Required]
        public string Subject { get; set; } = "";

        [Required]
        public string Body { get; set; } = "";

        public int? Id { get; set; }

        public string? Firstname { get; set; }
        public string? LastName { get; set; }

        public string? GroupName { get; set; }
    }
}

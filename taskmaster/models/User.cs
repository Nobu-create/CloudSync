using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace CloudSync.Models
{
    public class User : IdentityUser
    {
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Full Name")]
        public string FullName => $"{FirstName} {LastName}";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<ToDo> Tasks { get; set; } = new List<ToDo>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}

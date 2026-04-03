using System.ComponentModel.DataAnnotations;

namespace CloudSync.Models
{
    public class ProjectMember
    {
        public int Id { get; set; }

        public int ProjectId { get; set; }
        public Project? Project { get; set; }

        public string UserId { get; set; } = string.Empty;
        public User? User { get; set; }

        [Display(Name = "Role")]
        public string Role { get; set; } = "member"; // "owner" or "member"

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}

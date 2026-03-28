using System.ComponentModel.DataAnnotations;

namespace CloudSync.Models
{
    public class Priority
    {
        [Key]
        public string PriorityId { get; set; } = string.Empty;

        [Required]
        public string Name { get; set; } = string.Empty;

        public string BadgeColor { get; set; } = string.Empty;
    }
}

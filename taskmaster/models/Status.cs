using System.ComponentModel.DataAnnotations;

namespace CloudSync.Models
{
    public class Status
    {
        [Key]
        public string StatusId { get; set; } = string.Empty;

        [Required]
        public string Name { get; set; } = string.Empty;
    }
}

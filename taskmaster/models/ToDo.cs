using System.ComponentModel.DataAnnotations;

namespace CloudSync.Models
{
    public class ToDo
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter a description.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter a due date.")]
        public DateTime? DueDate { get; set; }

        [Required(ErrorMessage = "Please select a category.")]
        public string CategoryId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a status.")]
        public string StatusId { get; set; } = string.Empty;

        [Display(Name = "Priority")]
        public string PriorityId { get; set; } = "medium";

        // Project this task belongs to (optional)
        public int? ProjectId { get; set; }

        // Who created the task
        public string UserId { get; set; } = string.Empty;

        // Who is assigned to the task
        [Display(Name = "Assigned To")]
        public string? AssignedToUserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Category? Category { get; set; }
        public Status? Status { get; set; }
        public Priority? Priority { get; set; }
        public User? User { get; set; }
        public User? AssignedTo { get; set; }
        public Project? Project { get; set; }

        public bool Overdue => StatusId == "open" && DueDate < DateTime.Today;
    }
}


using System.ComponentModel.DataAnnotations;

namespace CloudSync.Models
{
    public class Project
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter a project name.")]
        [Display(Name = "Project Name")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Due Date")]
        public DateTime? DueDate { get; set; }

        [Display(Name = "Status")]
        public string StatusId { get; set; } = "active";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Owner of the project
        public string UserId { get; set; } = string.Empty;
        public User? Owner { get; set; }

        // Tasks in this project
        public ICollection<ToDo> Tasks { get; set; } = new List<ToDo>();

        // Members of this project
        public ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();

        // Computed properties
        public int TotalTasks => Tasks.Count;
        public int CompletedTasks => Tasks.Count(t => t.StatusId == "closed");
        public int OverdueTasks => Tasks.Count(t => t.Overdue);
        public double ProgressPercent => TotalTasks == 0 ? 0 : Math.Round((double)CompletedTasks / TotalTasks * 100, 0);
    }
}

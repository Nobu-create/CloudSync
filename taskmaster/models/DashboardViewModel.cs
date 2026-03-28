namespace CloudSync.Models
{
    public class DashboardViewModel
    {
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int OverdueTasks { get; set; }
        public int HighPriorityTasks { get; set; }
        public int TotalProjects { get; set; }
        public int ActiveProjects { get; set; }
        public List<ToDo> RecentTasks { get; set; } = new();
        public List<Project> Projects { get; set; } = new();
        public Dictionary<string, int> TasksByPriority { get; set; } = new();
        public Dictionary<string, int> TasksByStatus { get; set; } = new();
        public double CompletionRate => TotalTasks == 0 ? 0 : Math.Round((double)CompletedTasks / TotalTasks * 100, 0);
    }
}

using Microsoft.AspNetCore.Mvc.Rendering;

namespace CloudSync.Models
{
    public class TaskViewModel
    {
        public IEnumerable<ToDo> Tasks { get; set; } = new List<ToDo>();
        public Filters CurrentFilters { get; set; } = new Filters("all-all-all");
        public List<SelectListItem> Categories { get; set; } = new();
        public List<SelectListItem> Statuses { get; set; } = new();
        public List<SelectListItem> DueFilters { get; set; } = new();
        public List<SelectListItem> Priorities { get; set; } = new();
        public List<SelectListItem> Projects { get; set; } = new();
        public List<SelectListItem> TeamMembers { get; set; } = new();
    }
}

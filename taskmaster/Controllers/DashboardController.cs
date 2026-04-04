using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CloudSync.Models;

namespace CloudSync.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ToDoContext _context;
        private readonly UserManager<User> _userManager;

        public DashboardController(ToDoContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var tasks = await _context.ToDos
                .Include(t => t.Priority)
                .Include(t => t.Category)
                .Include(t => t.Status)
                .Include(t => t.Project)
                .Where(t => t.UserId == userId || t.AssignedToUserId == userId)
                .ToListAsync();

            var projects = await _context.Projects
                .Include(p => p.Tasks)
                .Include(p => p.Members)
                .Where(p => p.UserId == userId || p.Members.Any(m => m.UserId == userId))
                .ToListAsync();

            var viewModel = new DashboardViewModel
            {
                TotalTasks = tasks.Count,
                CompletedTasks = tasks.Count(t => t.StatusId == "closed"),
                OverdueTasks = tasks.Count(t => t.Overdue),
                HighPriorityTasks = tasks.Count(t => t.PriorityId == "high" && t.StatusId == "open"),
                TotalProjects = projects.Count,
                ActiveProjects = projects.Count(p => p.StatusId == "active"),
                RecentTasks = tasks.OrderByDescending(t => t.CreatedAt).Take(5).ToList(),
                Projects = projects,
                TasksByPriority = new Dictionary<string, int>
                {
                    { "High", tasks.Count(t => t.PriorityId == "high") },
                    { "Medium", tasks.Count(t => t.PriorityId == "medium") },
                    { "Low", tasks.Count(t => t.PriorityId == "low") }
                },
                TasksByStatus = new Dictionary<string, int>
                {
                    { "Open", tasks.Count(t => t.StatusId == "open") },
                    { "Closed", tasks.Count(t => t.StatusId == "closed") }
                }
            };

            return View(viewModel);
        }
    }
}

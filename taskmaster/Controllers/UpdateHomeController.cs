using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CloudSync.Models;


namespace CloudSync.Controllers
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ToDoContext _context;
        private readonly UserManager<User> _userManager;

        public HomeController(ToDoContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string id = "all-all-all")
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var filters = new Filters(id);
            var viewModel = new TaskViewModel
            {
                CurrentFilters = filters,
                Categories = await GetCategoriesSelectList(),
                Statuses = await GetStatusesSelectList(),
                DueFilters = GetDueFiltersSelectList(),
                Priorities = await GetPrioritiesSelectList()
            };

            var query = _context.ToDos
                .Include(t => t.Category)
                .Include(t => t.Status)
                .Include(t => t.Priority)
                .Include(t => t.Project)
                .Include(t => t.AssignedTo)
                .Where(t => t.UserId == userId || t.AssignedToUserId == userId)
                .AsQueryable();

            if (filters.HasCategory)
                query = query.Where(t => t.CategoryId == filters.CategoryId);
            if (filters.HasStatus)
                query = query.Where(t => t.StatusId == filters.StatusId);
            if (filters.HasDue)
            {
                var today = DateTime.Today;
                if (filters.IsPast) query = query.Where(t => t.DueDate < today);
                else if (filters.IsFuture) query = query.Where(t => t.DueDate > today);
                else if (filters.IsToday) query = query.Where(t => t.DueDate == today);
            }

            viewModel.Tasks = await query.OrderBy(t => t.DueDate).ToListAsync();
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Add()
        {
            var userId = _userManager.GetUserId(User);
            await PopulateViewBag(userId!);
            return View(new ToDo { StatusId = "open", PriorityId = "medium", DueDate = DateTime.Today.AddDays(1) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(ToDo task)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                task.UserId = userId;
                task.CreatedAt = DateTime.UtcNow;
                _context.ToDos.Add(task);
                await _context.SaveChangesAsync();

                // Send notification to assigned user
                if (!string.IsNullOrEmpty(task.AssignedToUserId) && task.AssignedToUserId != userId)
                {
                    var assignedUser = await _userManager.FindByIdAsync(task.AssignedToUserId);
                    var currentUser = await _userManager.FindByIdAsync(userId);
                    if (assignedUser != null && currentUser != null)
                    {
                        var notification = new Notification
                        {
                            UserId = task.AssignedToUserId,
                            Message = $"{currentUser.FullName} assigned you a task: \"{task.Description}\"",
                            Type = "info",
                            Link = "/Home/Index"
                        };
                        _context.Notifications.Add(notification);
                        await _context.SaveChangesAsync();
                    }
                }

                TempData["Success"] = "Task added successfully!";
                return RedirectToAction("Index");
            }

            await PopulateViewBag(userId!);
            return View(task);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Filter(string categoryFilter = "all", string dueFilter = "all", string statusFilter = "all")
        {
            return RedirectToAction("Index", new { id = $"{categoryFilter}-{dueFilter}-{statusFilter}" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkComplete(int id, string returnUrl = "all-all-all")
        {
            var userId = _userManager.GetUserId(User);
            var task = await _context.ToDos.FirstOrDefaultAsync(t => t.Id == id && (t.UserId == userId || t.AssignedToUserId == userId));
            if (task != null)
            {
                task.StatusId = "closed";
                await _context.SaveChangesAsync();
                TempData["Success"] = "Task marked as complete!";
            }
            return RedirectToAction("Index", new { id = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCompleted(string returnUrl = "all-all-all")
        {
            var userId = _userManager.GetUserId(User);
            var completed = await _context.ToDos.Where(t => t.StatusId == "closed" && t.UserId == userId).ToListAsync();
            if (completed.Any())
            {
                _context.ToDos.RemoveRange(completed);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Deleted {completed.Count} completed task(s).";
            }
            else TempData["Info"] = "No completed tasks to delete.";
            return RedirectToAction("Index", new { id = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, string returnUrl = "all-all-all")
        {
            var userId = _userManager.GetUserId(User);
            var task = await _context.ToDos.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
            if (task != null)
            {
                _context.ToDos.Remove(task);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Task deleted successfully!";
            }
            return RedirectToAction("Index", new { id = returnUrl });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User);
            var task = await _context.ToDos.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
            if (task == null) { TempData["Error"] = "Task not found."; return RedirectToAction("Index"); }
            await PopulateViewBag(userId!);
            return View(task);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ToDo task)
        {
            var userId = _userManager.GetUserId(User);
            var existing = await _context.ToDos.FirstOrDefaultAsync(t => t.Id == task.Id && t.UserId == userId);
            if (existing == null) { TempData["Error"] = "Task not found."; return RedirectToAction("Index"); }

            if (ModelState.IsValid)
            {
                existing.Description = task.Description;
                existing.DueDate = task.DueDate;
                existing.CategoryId = task.CategoryId;
                existing.StatusId = task.StatusId;
                existing.PriorityId = task.PriorityId;
                existing.ProjectId = task.ProjectId;
                existing.AssignedToUserId = task.AssignedToUserId;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Task updated successfully!";
                return RedirectToAction("Index");
            }

            await PopulateViewBag(userId!);
            return View(task);
        }

        // --- Helper methods ---
        private async Task PopulateViewBag(string userId)
        {
            ViewBag.Categories = await GetCategoriesSelectList();
            ViewBag.Statuses = await GetStatusesSelectList();
            ViewBag.Priorities = await GetPrioritiesSelectList();
            ViewBag.Projects = await GetProjectsSelectList(userId);
            ViewBag.TeamMembers = await GetTeamMembersSelectList(userId);
        }

        private async Task<List<SelectListItem>> GetCategoriesSelectList() =>
            (await _context.Categories.ToListAsync()).Select(c => new SelectListItem { Value = c.CategoryId, Text = c.Name }).ToList();

        private async Task<List<SelectListItem>> GetStatusesSelectList() =>
            (await _context.Statuses.ToListAsync()).Select(s => new SelectListItem { Value = s.StatusId, Text = s.Name }).ToList();

        private async Task<List<SelectListItem>> GetPrioritiesSelectList() =>
            (await _context.Priorities.ToListAsync()).Select(p => new SelectListItem { Value = p.PriorityId, Text = p.Name }).ToList();

        private async Task<List<SelectListItem>> GetProjectsSelectList(string userId) =>
            (await _context.Projects
                .Where(p => p.UserId == userId || p.Members.Any(m => m.UserId == userId))
                .ToListAsync())
            .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name }).ToList();

        private async Task<List<SelectListItem>> GetTeamMembersSelectList(string userId)
        {
            var memberIds = await _context.ProjectMembers
                .Where(pm => pm.Project.UserId == userId || pm.Project.Members.Any(m => m.UserId == userId))
                .Select(pm => pm.UserId)
                .Distinct()
                .ToListAsync();

            var members = await _userManager.Users.Where(u => memberIds.Contains(u.Id)).ToListAsync();
            return members.Select(u => new SelectListItem { Value = u.Id, Text = u.FullName }).ToList();
        }

        private static List<SelectListItem> GetDueFiltersSelectList() =>
            Filters.DueFilterValues.Select(kvp => new SelectListItem { Value = kvp.Key, Text = kvp.Value }).ToList();
    }
}

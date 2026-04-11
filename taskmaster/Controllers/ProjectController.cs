using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CloudSync.Models;

namespace CloudSync.Controllers
{
    [Authorize]
    public class ProjectController : Controller
    {
        private readonly ToDoContext _context;
        private readonly UserManager<User> _userManager;

        public ProjectController(ToDoContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // List all projects for the current user
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var projects = await _context.Projects
                .Include(p => p.Tasks)
                .Include(p => p.Members)
                    .ThenInclude(m => m.User)
                .Where(p => p.UserId == userId ||
                            p.Members.Any(m => m.UserId == userId))
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(projects);
        }

        // Project details page
        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);

            var project = await _context.Projects
                .Include(p => p.Tasks)
                    .ThenInclude(t => t.Priority)
                .Include(p => p.Tasks)
                    .ThenInclude(t => t.Category)
                .Include(p => p.Tasks)
                    .ThenInclude(t => t.Status)
                .Include(p => p.Tasks)
                    .ThenInclude(t => t.AssignedTo)
                .Include(p => p.Members)
                    .ThenInclude(m => m.User)
                .Include(p => p.Owner)
                .FirstOrDefaultAsync(p => p.Id == id &&
                    (p.UserId == userId || p.Members.Any(m => m.UserId == userId)));

            if (project == null)
            {
                TempData["Error"] = "Project not found.";
                return RedirectToAction("Index");
            }

            return View(project);
        }

        // Create project - GET
        [HttpGet]
        public IActionResult Create()
        {
            return View(new Project());
        }

        // Create project - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Project project)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                project.UserId = userId;
                project.CreatedAt = DateTime.UtcNow;
                project.StatusId = "active";

                _context.Projects.Add(project);
                await _context.SaveChangesAsync();

                // Add owner as a member too
                var member = new ProjectMember
                {
                    ProjectId = project.Id,
                    UserId = userId,
                    Role = "owner"
                };
                _context.ProjectMembers.Add(member);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Project '{project.Name}' created successfully!";
                return RedirectToAction("Details", new { id = project.Id });
            }

            return View(project);
        }

        // Edit project - GET
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User);
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (project == null)
            {
                TempData["Error"] = "Project not found.";
                return RedirectToAction("Index");
            }

            return View(project);
        }

        // Edit project - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Project project)
        {
            var userId = _userManager.GetUserId(User);
            var existing = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == project.Id && p.UserId == userId);

            if (existing == null)
            {
                TempData["Error"] = "Project not found.";
                return RedirectToAction("Index");
            }

            if (ModelState.IsValid)
            {
                existing.Name = project.Name;
                existing.Description = project.Description;
                existing.DueDate = project.DueDate;
                existing.StatusId = project.StatusId;

                await _context.SaveChangesAsync();
                TempData["Success"] = "Project updated successfully!";
                return RedirectToAction("Details", new { id = project.Id });
            }

            return View(project);
        }

        // Delete project - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (project != null)
            {
                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Project deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Project not found.";
            }

            return RedirectToAction("Index");
        }

        // Add member to project
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMember(int projectId, string email)
        {
            var userId = _userManager.GetUserId(User);
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == projectId && p.UserId == userId);

            if (project == null)
            {
                TempData["Error"] = "Project not found.";
                return RedirectToAction("Index");
            }

            var newMember = await _userManager.FindByEmailAsync(email);
            if (newMember == null)
            {
                TempData["Error"] = $"No user found with email '{email}'.";
                return RedirectToAction("Details", new { id = projectId });
            }

            // Check if already a member
            var alreadyMember = await _context.ProjectMembers
                .AnyAsync(m => m.ProjectId == projectId && m.UserId == newMember.Id);

            if (alreadyMember)
            {
                TempData["Error"] = "This user is already a member of this project.";
                return RedirectToAction("Details", new { id = projectId });
            }

            var member = new ProjectMember
            {
                ProjectId = projectId,
                UserId = newMember.Id,
                Role = "member"
            };

            _context.ProjectMembers.Add(member);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"{newMember.FullName} added to the project!";
            return RedirectToAction("Details", new { id = projectId });
        }

        // Remove member from project
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveMember(int projectId, string memberId)
        {
            var userId = _userManager.GetUserId(User);
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == projectId && p.UserId == userId);

            if (project == null)
            {
                TempData["Error"] = "Project not found.";
                return RedirectToAction("Index");
            }

            var member = await _context.ProjectMembers
                .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == memberId);

            if (member != null)
            {
                _context.ProjectMembers.Remove(member);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Member removed from project.";
            }

            return RedirectToAction("Details", new { id = projectId });
        }
    }
}

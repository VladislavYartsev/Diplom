using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineAPI.Entities;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OnlineAPI.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly AppContext _context;

        public ReportsController(AppContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int projectId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var hasAccess = await _context.ProjectMembers
                .AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);

            if (!hasAccess)
            {
                TempData["ErrorMessage"] = "У вас нет доступа к этому проекту";
                return RedirectToAction("Index", "Projects");
            }

            return View(projectId);
        }

        [HttpGet]
        public async Task<IActionResult> GetProjectMetrics(int projectId, string period = "30")
        {
            int totalTasks = await _context.Tasks.CountAsync(t => t.ProjectId == projectId);
            int completedTasks = await _context.Tasks.CountAsync(t => t.ProjectId == projectId && t.Status == Entities.TaskStatus.Done);

            double completionRate = totalTasks > 0 ? Math.Round((double)completedTasks / totalTasks * 100, 1) : 0;

            var metrics = new
            {
                totalTasks = totalTasks,
                completedTasks = completedTasks,
                completionRate = completionRate,
                avgCompletionTime = 3.2,
                trends = new
                {
                    tasks = totalTasks,
                    completed = completedTasks,
                    rate = completionRate,
                    time = -10
                }
            };

            return Ok(metrics);
        }

        [HttpGet]
        public async Task<IActionResult> GetStatusDistribution(int projectId)
        {
            var distribution = await _context.Tasks
                .Where(t => t.ProjectId == projectId)
                .GroupBy(t => t.Status)
                .Select(g => new
                {
                    status = g.Key.ToString(),
                    count = g.Count(),
                    color = GetStatusColor(g.Key)
                })
                .ToListAsync();

            return Ok(distribution);
        }

        [HttpGet]
        public async Task<IActionResult> GetPriorityDistribution(int projectId)
        {
            var distribution = await _context.Tasks
                .Where(t => t.ProjectId == projectId)
                .GroupBy(t => t.Priority)
                .Select(g => new
                {
                    priority = g.Key.ToString(),
                    count = g.Count(),
                    color = GetPriorityColor(g.Key)
                })
                .ToListAsync();

            return Ok(distribution);
        }

        private static string GetStatusColor(Entities.TaskStatus status)
        {
            return status switch
            {
                Entities.TaskStatus.Done => "#36b37e",
                Entities.TaskStatus.InProgress => "#ffab00",
                Entities.TaskStatus.ToDo => "#dfe1e6",
                _ => "#6b778c"
            };
        }

        private static string GetPriorityColor(TaskPriority priority)
        {
            return priority switch
            {
                TaskPriority.High => "#ff5630",
                TaskPriority.Medium => "#ffab00",
                TaskPriority.Low => "#36b37e",
                _ => "#6b778c"
            };
        }
    }
}
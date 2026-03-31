using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineAPI.Entities;
using System.Security.Claims;

namespace OnlineAPI.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly AppContext _context;
        private const int MAX_PROJECTS_PER_USER = 5;
        private const int MAX_OWNED_PROJECTS = 1;

        public ProjectsController(AppContext context)
        {
            _context = context;
        }

        // GET: /Projects
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User.Identity.Name;

            var userProjects = await _context.ProjectMembers
                .Where(pm => pm.UserId == userId)
                .Include(pm => pm.Project)
                    .ThenInclude(p => p.Tasks)
                .Include(pm => pm.Project)
                    .ThenInclude(p => p.Members)
                .Select(pm => new ProjectViewModel
                {
                    Project = pm.Project,
                    UserRole = pm.Role,
                    TaskCount = pm.Project.Tasks.Count,
                    MemberCount = pm.Project.Members.Count
                })
                .ToListAsync();

            var ownedProjectsCount = userProjects.Count(p => p.UserRole == ProjectRole.Owner);
            var canCreateProject = ownedProjectsCount < MAX_OWNED_PROJECTS;

            ViewBag.CanCreateProject = canCreateProject;
            ViewBag.OwnedProjectsCount = ownedProjectsCount;
            ViewBag.MaxOwnedProjects = MAX_OWNED_PROJECTS;
            ViewBag.TotalProjectsCount = userProjects.Count() ;
            ViewBag.MaxProjects = MAX_PROJECTS_PER_USER;

            return View(userProjects);
        }

        // GET: /Projects/Create
        [HttpGet]
        public IActionResult Create()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var ownedProjectsCount = _context.Projects.Count(p => p.OwnerId == userId);
            if (ownedProjectsCount >= MAX_OWNED_PROJECTS)
            {
                TempData["ErrorMessage"] = $"Вы не можете создать более {MAX_OWNED_PROJECTS} проекта";
                return RedirectToAction(nameof(Index));
            }

            return View();
        }

        // POST: /Projects/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateProjectViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User.Identity.Name;

            var ownedProjectsCount = await _context.Projects.CountAsync(p => p.OwnerId == userId);
            if (ownedProjectsCount >= MAX_OWNED_PROJECTS)
            {
                ModelState.AddModelError("", $"Вы не можете создать более {MAX_OWNED_PROJECTS} проекта");
                return View(model);
            }

            try
            {
                var project = new Project
                {
                    Name = model.Name,
                    Description = model.Description,
                    OwnerId = userId,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                };

                _context.Projects.Add(project);
                await _context.SaveChangesAsync();

                var projectMember = new ProjectMember
                {
                    ProjectId = project.Id,
                    UserId = userId,
                    UserName = userName,
                    Role = ProjectRole.Owner,
                    JoinedDate = DateTime.UtcNow
                };

                _context.ProjectMembers.Add(projectMember);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Проект успешно создан!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Ошибка при создании проекта: {ex.Message}");
                return View(model);
            }
        }

        // GET: /Projects/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var projectMember = await _context.ProjectMembers
                .Include(pm => pm.Project)
                    .ThenInclude(p => p.Members)
                .FirstOrDefaultAsync(pm => pm.ProjectId == id && pm.UserId == userId);

            if (projectMember == null)
            {
                TempData["ErrorMessage"] = "У вас нет доступа к этому проекту";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new ProjectDetailsViewModel
            {
                Project = projectMember.Project,
                UserRole = projectMember.Role,
                Tasks = projectMember.Project.Tasks.ToList()
            };

            return View(viewModel);
        }

        // POST: /Projects/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == id && p.OwnerId == userId);

            if (project == null)
            {
                TempData["ErrorMessage"] = "Проект не найден или у вас нет прав для его удаления";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var tasks = _context.Tasks.Where(t => t.ProjectId == id);
                _context.Tasks.RemoveRange(tasks);

                var members = _context.ProjectMembers.Where(pm => pm.ProjectId == id);
                _context.ProjectMembers.RemoveRange(members);

                _context.Projects.Remove(project);

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Проект успешно удален!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ошибка при удалении проекта: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }  
}
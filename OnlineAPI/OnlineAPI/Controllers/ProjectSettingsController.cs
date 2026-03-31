    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using OnlineAPI.Entities;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using System.Linq;

    namespace OnlineAPI.Controllers
    {
        [Authorize]
        public class ProjectSettingsController : Controller
        {
            private readonly AppContext _context;

            public ProjectSettingsController(AppContext context)
            {
                _context = context;
            }

            [HttpGet]
            public async Task<IActionResult> Index(int projectId)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var project = await _context.Projects
                    .Include(p => p.Members)
                    .FirstOrDefaultAsync(p => p.Id == projectId);

                if (project == null)
                {
                    TempData["ErrorMessage"] = "Проект не найден";
                    return RedirectToAction("Index", "Projects");
                }

                var userMember = project.Members.FirstOrDefault(m => m.UserId == userId);
                if (userMember == null)
                {
                    TempData["ErrorMessage"] = "У вас нет доступа к этому проекту";
                    return RedirectToAction("Index", "Projects");
                }

                var isOwner = userMember.Role == ProjectRole.Owner;
                var canEdit = isOwner || userMember.Role == ProjectRole.Admin;

                var viewModel = new ProjectSettingsViewModel
                {
                    Id = project.Id,
                    Name = project.Name,
                    Description = project.Description ?? "",
                    IsOwner = isOwner,
                    CanEdit = canEdit,
                    Members = project.Members.Select(m => new ProjectMemberViewModel
                    {
                        UserId = m.UserId,
                        UserName = m.UserName,
                        Role = m.Role.ToString(),
                        IsCurrentUser = m.UserId == userId,
                        CanChangeRole = isOwner && m.Role != ProjectRole.Owner && m.UserId != userId
                    }).ToList()
                };

                return View(viewModel);
            }

            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> UpdateProject(UpdateProjectRequest request)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var project = await _context.Projects
                    .Include(p => p.Members)
                    .FirstOrDefaultAsync(p => p.Id == request.ProjectId);

                if (project == null)
                {
                    return Json(new { success = false, message = "Проект не найден" });
                }


                var userMember = project.Members.FirstOrDefault(m => m.UserId == userId);
                var canEdit = userMember?.Role == ProjectRole.Owner|| userMember?.Role == ProjectRole.Admin;

                if (!canEdit)
                {
                    return Json(new { success = false, message = "Недостаточно прав для редактирования проекта" });
                }

                project.Name = request.Name;
                project.Description = request.Description;
                project.UpdatedDate = DateTime.UtcNow;

                _context.Projects.Update(project);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Настройки проекта обновлены" });
            }

            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> UpdateMemberRole(UpdateMemberRoleRequest request)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var project = await _context.Projects
                    .Include(p => p.Members)
                    .FirstOrDefaultAsync(p => p.Id == request.ProjectId);

                if (project == null)
                {
                    return Json(new { success = false, message = "Проект не найден" });
                }

                var currentUserMember = project.Members.FirstOrDefault(m => m.UserId == userId);
                if (currentUserMember?.Role != ProjectRole.Owner)
                {
                    return Json(new { success = false, message = "Только владелец проекта может изменять роли" });
                }

                var targetMember = project.Members.FirstOrDefault(m => m.UserId == request.UserId);
                if (targetMember == null)
                {
                    return Json(new { success = false, message = "Участник не найден" });
                }

                if (targetMember.Role == ProjectRole.Owner || request.UserId == userId)
                {
                    return Json(new { success = false, message = "Нельзя изменить роль владельца или свою собственную роль" });
                }

                targetMember.Role = request.NewRole;
                _context.ProjectMembers.Update(targetMember);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Роль участника обновлена" });
            }

            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> RemoveMember(RemoveMemberRequest request)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var project = await _context.Projects
                    .Include(p => p.Members)
                    .FirstOrDefaultAsync(p => p.Id == request.ProjectId);

                if (project == null)
                {
                    return Json(new { success = false, message = "Проект не найден" });
                }

                var currentUserMember = project.Members.FirstOrDefault(m => m.UserId == userId);
                if (currentUserMember?.Role != ProjectRole.Owner)
                {
                    return Json(new { success = false, message = "Только владелец проекта может удалять участников" });
                }

                var targetMember = project.Members.FirstOrDefault(m => m.UserId == request.UserId);
                if (targetMember == null)
                {
                    return Json(new { success = false, message = "Участник не найден" });
                }

                if (targetMember.Role == ProjectRole.Owner || request.UserId == userId)
                {
                    return Json(new { success = false, message = "Нельзя удалить владельца или себя из проекта" });
                }

                _context.ProjectMembers.Remove(targetMember);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Участник удален из проекта" });
            }
        }
    }
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineAPI.Entities;
using System.Security.Claims;

namespace OnlineAPI.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly AppContext _context;

        public ProfileController(AppContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id.ToString() == userId);
            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var projectMemberships = await _context.ProjectMembers
                .Where(pm => pm.UserId == userId)
                .Include(pm => pm.Project)
                    .ThenInclude(p => p.Tasks)
                .ToListAsync();

            var model = new ProfileViewModel
            {
                Id = user.Id,
                Username = user.Username,
                InvitationCode = user.InvintaionCode,
                ProjectCount = projectMemberships.Count,
                OwnedProjectCount = projectMemberships.Count(pm => pm.Role == ProjectRole.Owner),
                TaskCount = projectMemberships
                    .Where(pm => pm.Project?.Tasks != null)
                    .Sum(pm => pm.Project.Tasks.Count)
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id.ToString() == userId);
            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var model = new EditProfileViewModel
            {
                Id = user.Id,
                Username = user.Username
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditProfileViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId) || userId != model.Id.ToString())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id.ToString() == userId);
            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var duplicateUser = await _context.Users
                .AnyAsync(u => u.Username == model.Username && u.Id != user.Id);

            if (duplicateUser)
            {
                ModelState.AddModelError(nameof(model.Username), "Пользователь с таким логином уже существует");
                return View(model);
            }

            user.Username = model.Username;
            if (!string.IsNullOrWhiteSpace(model.NewPassword))
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            }

            await _context.SaveChangesAsync();

            var identity = User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                var nameClaim = identity.FindFirst(ClaimTypes.Name);
                if (nameClaim != null)
                {
                    identity.RemoveClaim(nameClaim);
                }
                identity.AddClaim(new Claim(ClaimTypes.Name, user.Username));
                await HttpContext.SignInAsync("UserScheme", new ClaimsPrincipal(identity));
            }

            TempData["SuccessMessage"] = "Профиль обновлен";
            return RedirectToAction(nameof(Index));
        }
    }
}

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using OnlineAPI.DTOs;
using OnlineAPI.Entities;
using System.Security.Claims;


namespace OnlineAPI.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppContext _context;
        public AuthController(AppContext db)
        {
            _context = db;
        }

        [HttpGet]
        public IActionResult Login()
        {

            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Tasks");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(Entities.LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == model.Username);

            // Проверяем пароль через BCrypt
            if (user != null && BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
            {
                const string UserScheme = "UserScheme";

                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
        };

                var claimsIdentity = new ClaimsIdentity(claims, UserScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                    ExpiresUtc = model.RememberMe
                        ? DateTime.UtcNow.AddDays(30)
                        : DateTime.UtcNow.AddMinutes(30)
                };

                await HttpContext.SignInAsync(
                    UserScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                return RedirectToAction("Index", "Tasks");
            }

            ModelState.AddModelError("", "Invalid login attempt");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {

            const string UserScheme = "UserScheme";
            await HttpContext.SignOutAsync(UserScheme);
            return RedirectToAction("Login", "Auth");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CheckCode(string code)
        {
            var inv = _context.Invitations.FirstOrDefault(i => i.Code == code && !i.IsUsed);

            if (inv == null)
                return Json(new { success = false, message = "Код недействителен" });

            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult Register(string login, string password, string code)
        {
            var inv = _context.Invitations.FirstOrDefault(i => i.Code == code && !i.IsUsed);
            if (inv == null) return BadRequest();

            _context.Users.Add(new User
            {
                Username = login,
                Password = BCrypt.Net.BCrypt.HashPassword(password),
                InvintaionCode = code
            });

            inv.IsUsed = true;
            _context.SaveChanges();

            return Redirect("/auth/login");
        }

    }
}
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineAPI.Entities;
using System.Security.Claims;
namespace OnlineAPI
{
    public class SuperAdminController : Controller
    {
        private readonly AppContext _context;
        public SuperAdminController(AppContext context)
        {
            _context = context;
        }


        [Authorize(AuthenticationSchemes = "SuperAdminScheme")]
        public IActionResult Index()
        {
            var users = _context.Users.ToList();
            return View(users);
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string login, string password)
        {
            if (login == "superadmin" && password == "Super123!")
            {
                var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, login),
                new Claim(ClaimTypes.Role, "SuperAdmin")
            };

                const string SuperAdminScheme = "SuperAdminScheme";

                var identity = new ClaimsIdentity(
                    claims, SuperAdminScheme);

                await HttpContext.SignInAsync(
                    SuperAdminScheme,
                    new ClaimsPrincipal(identity),
                    new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30)
                    });


                return RedirectToAction("Index");
            }

            ViewBag.Error = "Неверный логин или пароль";
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("SuperAdminScheme");
            return RedirectToAction("Login");
        }

        [HttpPost]
        public async Task<IActionResult> GenerateCode()
        {
            var Code = GenerateInviteCode();

            var Invite = new Invitation
            {
                Code = Code,
                IsUsed = false

            };
            await _context.Invitations.AddAsync(Invite);
            await _context.SaveChangesAsync();

            return Json(new { Code });
        }

        private string GenerateInviteCode()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var random = new Random();

            return new string(Enumerable
                .Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)])
                .ToArray()
                );


        }

       

    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace OnlineAPI.Views.Auth
{
    public class loginModel : PageModel
    {
        [BindProperty]
        [Required(ErrorMessage = "Введите имя пользователя")]
        public string Username { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Введите пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Добавьте здесь вашу логику аутентификации
            // Пример проверки учетных данных:
            if (Username == "admin" && Password == "password")
            {
                // Вход выполнен успешно
                return RedirectToPage("/Index");
            }

            ModelState.AddModelError(string.Empty, "Неверные учетные данные");
            return Page();
        }
    }
}
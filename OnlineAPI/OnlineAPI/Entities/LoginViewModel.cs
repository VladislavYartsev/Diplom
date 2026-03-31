using OnlineAPI.Entities;
using System.ComponentModel.DataAnnotations;

namespace OnlineAPI.Entities
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Имя пользователя обязательно")]

        public string Username { get; set; }

        [Required(ErrorMessage = "Пароль обязателен")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }


}

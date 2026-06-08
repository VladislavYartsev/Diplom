using System.ComponentModel.DataAnnotations;

namespace OnlineAPI.Entities
{
    public class ProfileViewModel
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string? InvitationCode { get; set; }
        public int ProjectCount { get; set; }
        public int OwnedProjectCount { get; set; }
        public int TaskCount { get; set; }
    }

    public class EditProfileViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Логин обязателен")]
        [StringLength(50, ErrorMessage = "Логин не должен превышать 50 символов")]
        public string Username { get; set; }

        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Новый пароль должен содержать минимум 6 символов")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Пароли не совпадают")]
        public string? ConfirmPassword { get; set; }
    }
}

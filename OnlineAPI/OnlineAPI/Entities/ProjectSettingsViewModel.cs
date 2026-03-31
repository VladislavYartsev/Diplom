    using System.ComponentModel.DataAnnotations;
    using System.Collections.Generic;

namespace OnlineAPI.Entities
{
    public class ProjectSettingsViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Название проекта обязательно")]
        [StringLength(100, ErrorMessage = "Название не должно превышать 100 символов")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "Описание не должно превышать 500 символов")]
        public string Description { get; set; }

        public bool IsOwner { get; set; }
        public bool CanEdit { get; set; }

        public List<ProjectMemberViewModel> Members { get; set; } = new List<ProjectMemberViewModel>();
    }

    public class ProjectMemberViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Role { get; set; }
        public bool IsCurrentUser { get; set; }
        public bool CanChangeRole { get; set; }
    }

    public class UpdateProjectRequest
    {
        public int ProjectId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class UpdateMemberRoleRequest
    {
        public int ProjectId { get; set; }
        public string UserId { get; set; }
        public ProjectRole NewRole { get; set; }
    }

    public class RemoveMemberRequest
    {
        public int ProjectId { get; set; }
        public string UserId { get; set; }
    }
}

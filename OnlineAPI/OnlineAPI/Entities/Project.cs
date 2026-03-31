using System.ComponentModel.DataAnnotations;

namespace OnlineAPI.Entities
{
    public class Project
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Название проекта обязательно")]
        [StringLength(100, ErrorMessage = "Название не должно превышать 100 символов")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "Описание не должно превышать 500 символов")]
        public string Description { get; set; }

        public string OwnerId { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime UpdatedDate { get; set; }

        public ICollection<Entities.Task> Tasks { get; set; } = new List<Entities.Task>();

        public ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();
    }

   
    public enum ProjectRole
    {
        Owner,
        Admin,
        Member,
        Viewer
    }

    public class CreateProjectViewModel
    {
        [Required(ErrorMessage = "Название проекта обязательно")]
        [StringLength(100, ErrorMessage = "Название не должно превышать 100 символов")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "Описание не должно превышать 500 символов")]
        public string Description { get; set; }
    }
}
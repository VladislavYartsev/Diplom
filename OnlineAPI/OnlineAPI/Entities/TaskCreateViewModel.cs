using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineAPI.Entities
{

    public class TaskCreateViewModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public TaskPriority Priority { get; set; }
        public int ProjectID { get; set; }

        public string SelectedUsersData { get; set; } 
        [NotMapped]
        public string[] SelectedUsers
        {
            get => SelectedUsersData?.Split(',') ?? Array.Empty<string>();
            set => SelectedUsersData = string.Join(',', value);
        }
        public List<SelectListItem> AvailableUsers { get; set; } = new();

        

    }


}

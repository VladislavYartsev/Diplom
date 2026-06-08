using Microsoft.AspNetCore.Mvc.Rendering;

namespace OnlineAPI.Entities
{
    public class TaskEditViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public TaskStatus Status { get; set; }
        public TaskPriority Priority { get; set; }
        public DateTime? Deadline { get; set; }
        public int ProjectId { get; set; }
        public string[] SelectedUsers { get; set; } = Array.Empty<string>();
        public List<SelectListItem> AvailableUsers { get; set; } = new();
    }
}

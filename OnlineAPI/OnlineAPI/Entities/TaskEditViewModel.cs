using Microsoft.AspNetCore.Mvc.Rendering;

namespace OnlineAPI.Entities
{
    namespace OnlineAPI.Entities
    {
        public class TaskCreateViewModel
        {
            public int ID { get; set; }
            public string Title { get; set; }
            public string Discription { get; set; }
            public TaskPriority TaskPriority { get; set; }
            public int ProjectID { get; set; }
            public List<int> SelectedUsers { get; set; } = new();
            public List<SelectListItem> AvailableUsers { get; set; }

        }
    }

}

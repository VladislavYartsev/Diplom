using System.ComponentModel.DataAnnotations;
using System.Net.NetworkInformation;

namespace OnlineAPI.Entities
{

        public enum TaskStatus
        {
            ToDo,
            InProgress,
            Review,
            Done
        }

    public enum TaskPriority
    {
        Low = 0,
        Medium = 1,
        High = 2
    }

    public class Task
    {
        public int Id { get; set; }


        [StringLength(100)]
        public string Title { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public TaskStatus Status { get; set; }

        public TaskPriority Priority { get; set; }
        
        public string[] Assignee { get; set; } = Array.Empty<string>();

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedDate { get; set; }

        [StringLength(10)]
        public string? TaskCode { get; set; }
        public int ProjectId { get; set; }
    }

        public class TaskMoveRequest
        {
            public int TaskId { get; set; }
            public TaskStatus NewStatus { get; set; }
        }
}


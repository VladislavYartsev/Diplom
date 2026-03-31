namespace OnlineAPI.Entities
{
    public class ProjectViewModel
    {
        public Project Project { get; set; }
        public ProjectRole UserRole { get; set; }
        public int TaskCount { get; set; }
        public int MemberCount { get; set; }
    }
    public class ProjectDetailsViewModel
    {
        public Project Project { get; set; }
        public ProjectRole UserRole { get; set; }
        public List<Entities.Task> Tasks { get; set; }
    }
}

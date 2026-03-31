namespace OnlineAPI.Entities
{
    public class ProjectMember
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public ProjectRole Role { get; set; }
        public DateTime JoinedDate { get; set; }
        public virtual Project Project { get; set; }
    }
}

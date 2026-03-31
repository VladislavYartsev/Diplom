namespace OnlineAPI.Entities
{
    public class Invitation
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public bool IsUsed { get; set; }
    }
}

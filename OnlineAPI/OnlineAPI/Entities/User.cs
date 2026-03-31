namespace OnlineAPI.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Username { get; set; }
    public string Password { get; set; }
    public string? InvintaionCode { get; set; }
    public int? OwnedProjectID {  get; set; }
    
}

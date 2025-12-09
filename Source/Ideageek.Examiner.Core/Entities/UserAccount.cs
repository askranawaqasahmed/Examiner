namespace Ideageek.Examiner.Core.Entities;

public class UserAccount : IEntity
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public Guid? StudentId { get; set; }
    public Guid? TeacherId { get; set; }
    public DateTime CreatedAt { get; set; }
}

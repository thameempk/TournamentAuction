namespace TournamentAuction.Domain.Entities;

public class User
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string UserType { get; set; } = string.Empty; // Admin, TournamentAdmin
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

namespace TournamentAuction.Domain.Entities;

public class Player
{
    public Guid PlayerId { get; set; }
    public Guid TournamentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid Category { get; set; } // CategoryId GUID
    public decimal BasePrice { get; set; }
    public Guid? AssignedTeamId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

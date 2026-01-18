namespace TournamentAuction.Domain.Entities;

public class Match
{
    public Guid MatchId { get; set; }
    public Guid TournamentId { get; set; }
    public Guid TeamAId { get; set; }
    public Guid TeamBId { get; set; }
    public Guid? WinningTeamId { get; set; }
    public DateTime ScheduledAt { get; set; }
    public string Status { get; set; } = "Pending"; // Pending / Completed / Cancelled
    public Guid CreatedBy { get; set; }
    public Guid UpdatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

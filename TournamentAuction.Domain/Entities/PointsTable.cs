namespace TournamentAuction.Domain.Entities;

public class PointsTable
{
    public Guid PointId { get; set; }
    public Guid TeamId { get; set; }
    public Guid TournamentId { get; set; }
    public int Points { get; set; }
    public int GoalsScored { get; set; }
    public int GoalsConceded { get; set; }
    public decimal? GoalsOrNRR { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int Draws { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid UpdatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

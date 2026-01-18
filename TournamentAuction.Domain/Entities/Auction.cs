namespace TournamentAuction.Domain.Entities;

public class Auction
{
    public Guid AuctionId { get; set; }
    public Guid TournamentId { get; set; }
    public string Status { get; set; } = "Pending"; // Pending / InProgress / Paused / Completed
    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid UpdatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

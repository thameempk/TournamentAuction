namespace TournamentAuction.Domain.Entities;

public class Tournament
{
    public Guid TournamentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid TournamentAdminId { get; set; }
    public Guid Type { get; set; } // TournamentType GUID
    public string Format { get; set; } = string.Empty; // League / Knockout / Mixed
    public bool AuctionEnabled { get; set; }
    public int MinPlayers { get; set; }
    public int MaxPlayers { get; set; }
    public decimal? MinBidIncrement { get; set; }
    public decimal? MaxTeamBudget { get; set; }
    public int? BidSeconds { get; set; }
    public string Status { get; set; } = "Draft"; // Draft / Configured / InProgress / Completed
    public Guid CreatedBy { get; set; }
    public Guid UpdatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

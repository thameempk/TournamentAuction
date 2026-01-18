namespace TournamentAuction.Domain.Entities;

public class Team
{
    public Guid TeamId { get; set; }
    public Guid TournamentId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public string TeamKey { get; set; } = string.Empty;
    public decimal? Wallet { get; set; }
    public string? LogoUrl { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid UpdatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

namespace TournamentAuction.Domain.Entities;

public class PlayerCategory
{
    public Guid CategoryId { get; set; }
    public Guid TournamentId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public Guid CreatedBy { get; set; }
    public Guid UpdatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

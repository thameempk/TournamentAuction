namespace TournamentAuction.Domain.Entities;

public class SubscriptionPlan
{
    public Guid PlanId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int DurationInDays { get; set; }
    public int? MaxTournaments { get; set; }
    public int? MaxTeams { get; set; }
    public int? MaxAuctions { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

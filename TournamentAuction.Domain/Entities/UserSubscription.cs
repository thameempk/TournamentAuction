namespace TournamentAuction.Domain.Entities;

public class UserSubscription
{
    public Guid SubscriptionId { get; set; }
    public Guid UserId { get; set; }
    public Guid PlanId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = "Active"; // Active / Expired / Cancelled / Suspended
    public decimal AmountPaid { get; set; }
    public string? PaymentReference { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

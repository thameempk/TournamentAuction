using TournamentAuction.Domain.Entities;

namespace TournamentAuction.Domain.Interfaces;

public interface ISubscriptionPlanRepository
{
    Task<IEnumerable<SubscriptionPlan>> GetAllActiveAsync();
    Task<SubscriptionPlan?> GetByIdAsync(Guid planId);
    Task<SubscriptionPlan> CreateAsync(SubscriptionPlan plan);
    Task<SubscriptionPlan> UpdateAsync(SubscriptionPlan plan);
    Task<bool> DeleteAsync(Guid planId);
}


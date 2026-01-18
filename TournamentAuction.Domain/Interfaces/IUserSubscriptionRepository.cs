using TournamentAuction.Domain.Entities;

namespace TournamentAuction.Domain.Interfaces;

public interface IUserSubscriptionRepository
{
    Task<UserSubscription?> GetActiveByUserIdAsync(Guid userId);
    Task<UserSubscription?> GetByIdAsync(Guid subscriptionId);
    Task<IEnumerable<UserSubscription>> GetByUserIdAsync(Guid userId);
    Task<UserSubscription> CreateAsync(UserSubscription subscription);
    Task<UserSubscription> UpdateAsync(UserSubscription subscription);
    Task<bool> DeleteAsync(Guid subscriptionId);
}


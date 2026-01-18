using Dapper;
using TournamentAuction.Domain.Entities;
using TournamentAuction.Domain.Interfaces;
using TournamentAuction.Infrastructure.Data;

namespace TournamentAuction.Infrastructure.Repositories;

public class UserSubscriptionRepository : IUserSubscriptionRepository
{
    private readonly DapperContext _context;

    public UserSubscriptionRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<UserSubscription?> GetActiveByUserIdAsync(Guid userId)
    {
        var query = @"
            SELECT SubscriptionId, UserId, PlanId, StartDate, EndDate, Status, AmountPaid, PaymentReference, CreatedAt, UpdatedAt
            FROM UserSubscriptions
            WHERE UserId = @UserId AND Status = 'Active' AND EndDate >= GETDATE()
            ORDER BY CreatedAt DESC";

        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<UserSubscription>(query, new { UserId = userId });
    }

    public async Task<UserSubscription?> GetByIdAsync(Guid subscriptionId)
    {
        var query = @"
            SELECT SubscriptionId, UserId, PlanId, StartDate, EndDate, Status, AmountPaid, PaymentReference, CreatedAt, UpdatedAt
            FROM UserSubscriptions
            WHERE SubscriptionId = @SubscriptionId";

        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<UserSubscription>(query, new { SubscriptionId = subscriptionId });
    }

    public async Task<IEnumerable<UserSubscription>> GetByUserIdAsync(Guid userId)
    {
        var query = @"
            SELECT SubscriptionId, UserId, PlanId, StartDate, EndDate, Status, AmountPaid, PaymentReference, CreatedAt, UpdatedAt
            FROM UserSubscriptions
            WHERE UserId = @UserId
            ORDER BY CreatedAt DESC";

        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<UserSubscription>(query, new { UserId = userId });
    }

    public async Task<UserSubscription> CreateAsync(UserSubscription subscription)
    {
        if (subscription.SubscriptionId == Guid.Empty)
        {
            subscription.SubscriptionId = Guid.NewGuid();
        }

        subscription.CreatedAt = DateTime.UtcNow;
        subscription.UpdatedAt = DateTime.UtcNow;

        var query = @"
            INSERT INTO UserSubscriptions (SubscriptionId, UserId, PlanId, StartDate, EndDate, Status, AmountPaid, PaymentReference, CreatedAt, UpdatedAt)
            VALUES (@SubscriptionId, @UserId, @PlanId, @StartDate, @EndDate, @Status, @AmountPaid, @PaymentReference, @CreatedAt, @UpdatedAt)";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(query, subscription);
        return subscription;
    }

    public async Task<UserSubscription> UpdateAsync(UserSubscription subscription)
    {
        subscription.UpdatedAt = DateTime.UtcNow;

        var query = @"
            UPDATE UserSubscriptions
            SET Status = @Status, AmountPaid = @AmountPaid, PaymentReference = @PaymentReference, UpdatedAt = @UpdatedAt
            WHERE SubscriptionId = @SubscriptionId";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(query, subscription);
        return subscription;
    }

    public async Task<bool> DeleteAsync(Guid subscriptionId)
    {
        var query = "DELETE FROM UserSubscriptions WHERE SubscriptionId = @SubscriptionId";

        using var connection = _context.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(query, new { SubscriptionId = subscriptionId });
        return rowsAffected > 0;
    }
}


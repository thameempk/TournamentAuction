using Dapper;
using TournamentAuction.Domain.Entities;
using TournamentAuction.Domain.Interfaces;
using TournamentAuction.Infrastructure.Data;

namespace TournamentAuction.Infrastructure.Repositories;

public class SubscriptionPlanRepository : ISubscriptionPlanRepository
{
    private readonly DapperContext _context;

    public SubscriptionPlanRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SubscriptionPlan>> GetAllActiveAsync()
    {
        var query = @"
            SELECT PlanId, PlanName, Price, DurationInDays, MaxTournaments, MaxTeams, MaxAuctions, IsActive, CreatedAt
            FROM SubscriptionPlans
            WHERE IsActive = 1
            ORDER BY Price ASC";

        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<SubscriptionPlan>(query);
    }

    public async Task<SubscriptionPlan?> GetByIdAsync(Guid planId)
    {
        var query = @"
            SELECT PlanId, PlanName, Price, DurationInDays, MaxTournaments, MaxTeams, MaxAuctions, IsActive, CreatedAt
            FROM SubscriptionPlans
            WHERE PlanId = @PlanId";

        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<SubscriptionPlan>(query, new { PlanId = planId });
    }

    public async Task<SubscriptionPlan> CreateAsync(SubscriptionPlan plan)
    {
        if (plan.PlanId == Guid.Empty)
        {
            plan.PlanId = Guid.NewGuid();
        }

        var query = @"
            INSERT INTO SubscriptionPlans (PlanId, PlanName, Price, DurationInDays, MaxTournaments, MaxTeams, MaxAuctions, IsActive, CreatedAt)
            VALUES (@PlanId, @PlanName, @Price, @DurationInDays, @MaxTournaments, @MaxTeams, @MaxAuctions, @IsActive, @CreatedAt)";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(query, plan);
        return plan;
    }

    public async Task<SubscriptionPlan> UpdateAsync(SubscriptionPlan plan)
    {
        var query = @"
            UPDATE SubscriptionPlans
            SET PlanName = @PlanName, Price = @Price, DurationInDays = @DurationInDays,
                MaxTournaments = @MaxTournaments, MaxTeams = @MaxTeams, MaxAuctions = @MaxAuctions,
                IsActive = @IsActive
            WHERE PlanId = @PlanId";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(query, plan);
        return plan;
    }

    public async Task<bool> DeleteAsync(Guid planId)
    {
        var query = "DELETE FROM SubscriptionPlans WHERE PlanId = @PlanId";

        using var connection = _context.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(query, new { PlanId = planId });
        return rowsAffected > 0;
    }
}


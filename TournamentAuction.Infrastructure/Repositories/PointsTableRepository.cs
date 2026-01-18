using Dapper;
using TournamentAuction.Domain.Entities;
using TournamentAuction.Domain.Interfaces;
using TournamentAuction.Infrastructure.Data;

namespace TournamentAuction.Infrastructure.Repositories;

public class PointsTableRepository : IPointsTableRepository
{
    private readonly DapperContext _context;

    public PointsTableRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PointsTable>> GetByTournamentIdAsync(Guid tournamentId)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT * FROM PointsTable WHERE TournamentId = @TournamentId ORDER BY Points DESC, GoalsOrNRR DESC";
        return await connection.QueryAsync<PointsTable>(sql, new { TournamentId = tournamentId });
    }

    public async Task<PointsTable?> GetByTournamentAndTeamIdAsync(Guid tournamentId, Guid teamId)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT * FROM PointsTable WHERE TournamentId = @TournamentId AND TeamId = @TeamId";
        return await connection.QueryFirstOrDefaultAsync<PointsTable>(sql, new { TournamentId = tournamentId, TeamId = teamId });
    }

    public async Task<PointsTable> CreateOrUpdateAsync(PointsTable pointsTable)
    {
        using var connection = _context.CreateConnection();
        var existing = await GetByTournamentAndTeamIdAsync(pointsTable.TournamentId, pointsTable.TeamId);
        
        if (existing == null)
        {
            if (pointsTable.PointId == Guid.Empty)
            {
                pointsTable.PointId = Guid.NewGuid();
            }
            pointsTable.CreatedAt = DateTime.UtcNow;
            pointsTable.UpdatedAt = DateTime.UtcNow;
            
            var sql = @"INSERT INTO PointsTable (PointId, TeamId, TournamentId, Points, GoalsScored, GoalsConceded, GoalsOrNRR, Wins, Losses, Draws, CreatedBy, UpdatedBy, CreatedAt, UpdatedAt)
                        VALUES (@PointId, @TeamId, @TournamentId, @Points, @GoalsScored, @GoalsConceded, @GoalsOrNRR, @Wins, @Losses, @Draws, @CreatedBy, @UpdatedBy, @CreatedAt, @UpdatedAt)";
            await connection.ExecuteAsync(sql, pointsTable);
        }
        else
        {
            pointsTable.PointId = existing.PointId;
            pointsTable.UpdatedAt = DateTime.UtcNow;
            var sql = @"UPDATE PointsTable SET Points = @Points, GoalsScored = @GoalsScored, GoalsConceded = @GoalsConceded,
                        GoalsOrNRR = @GoalsOrNRR, Wins = @Wins, Losses = @Losses, Draws = @Draws, UpdatedBy = @UpdatedBy, UpdatedAt = @UpdatedAt
                        WHERE PointId = @PointId";
            await connection.ExecuteAsync(sql, pointsTable);
        }
        
        return pointsTable;
    }

    public async Task<bool> UpdateMatchResultAsync(Guid tournamentId, Guid team1Id, Guid team2Id, Guid? winnerId)
    {
        // This is handled by MatchService
        return true;
    }
}

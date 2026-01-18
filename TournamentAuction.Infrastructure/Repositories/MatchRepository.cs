using Dapper;
using TournamentAuction.Domain.Entities;
using TournamentAuction.Domain.Interfaces;
using TournamentAuction.Infrastructure.Data;

namespace TournamentAuction.Infrastructure.Repositories;

public class MatchRepository : IMatchRepository
{
    private readonly DapperContext _context;

    public MatchRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Match>> GetByTournamentIdAsync(Guid tournamentId)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT * FROM Matches WHERE TournamentId = @TournamentId ORDER BY ScheduledAt";
        return await connection.QueryAsync<Match>(sql, new { TournamentId = tournamentId });
    }

    public async Task<Match?> GetByIdAsync(Guid matchId)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT * FROM Matches WHERE MatchId = @MatchId";
        return await connection.QueryFirstOrDefaultAsync<Match>(sql, new { MatchId = matchId });
    }

    public async Task<Match> CreateAsync(Match match)
    {
        using var connection = _context.CreateConnection();
        if (match.MatchId == Guid.Empty)
        {
            match.MatchId = Guid.NewGuid();
        }
        match.CreatedAt = DateTime.UtcNow;
        match.UpdatedAt = DateTime.UtcNow;
        
        var sql = @"INSERT INTO Matches (MatchId, TournamentId, TeamAId, TeamBId, WinningTeamId, ScheduledAt, Status, CreatedBy, UpdatedBy, CreatedAt, UpdatedAt)
                    VALUES (@MatchId, @TournamentId, @TeamAId, @TeamBId, @WinningTeamId, @ScheduledAt, @Status, @CreatedBy, @UpdatedBy, @CreatedAt, @UpdatedAt)";
        await connection.ExecuteAsync(sql, match);
        return match;
    }

    public async Task<Match> UpdateAsync(Match match)
    {
        using var connection = _context.CreateConnection();
        match.UpdatedAt = DateTime.UtcNow;
        var sql = @"UPDATE Matches SET TeamAId = @TeamAId, TeamBId = @TeamBId, WinningTeamId = @WinningTeamId,
                    ScheduledAt = @ScheduledAt, Status = @Status, UpdatedBy = @UpdatedBy, UpdatedAt = @UpdatedAt
                    WHERE MatchId = @MatchId";
        await connection.ExecuteAsync(sql, match);
        return match;
    }

    public async Task<bool> DeleteAsync(Guid matchId)
    {
        using var connection = _context.CreateConnection();
        var sql = "DELETE FROM Matches WHERE MatchId = @MatchId";
        var rowsAffected = await connection.ExecuteAsync(sql, new { MatchId = matchId });
        return rowsAffected > 0;
    }
}

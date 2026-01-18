using Dapper;
using TournamentAuction.Domain.Entities;
using TournamentAuction.Domain.Interfaces;
using TournamentAuction.Infrastructure.Data;

namespace TournamentAuction.Infrastructure.Repositories;

public class TeamRepository : ITeamRepository
{
    private readonly DapperContext _context;

    public TeamRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Team>> GetByTournamentIdAsync(Guid tournamentId)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT * FROM Teams WHERE TournamentId = @TournamentId";
        return await connection.QueryAsync<Team>(sql, new { TournamentId = tournamentId });
    }

    public async Task<Team?> GetByIdAsync(Guid teamId)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT * FROM Teams WHERE TeamId = @TeamId";
        return await connection.QueryFirstOrDefaultAsync<Team>(sql, new { TeamId = teamId });
    }

    public async Task<Team?> GetByTeamKeyAsync(string teamKey)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT * FROM Teams WHERE TeamKey = @TeamKey";
        return await connection.QueryFirstOrDefaultAsync<Team>(sql, new { TeamKey = teamKey });
    }

    public async Task<Team> CreateAsync(Team team)
    {
        using var connection = _context.CreateConnection();
        if (team.TeamId == Guid.Empty)
        {
            team.TeamId = Guid.NewGuid();
        }
        if (string.IsNullOrEmpty(team.TeamKey))
        {
            team.TeamKey = Guid.NewGuid().ToString("N")[..8].ToUpper();
        }
        team.CreatedAt = DateTime.UtcNow;
        team.UpdatedAt = DateTime.UtcNow;
        
        var sql = @"INSERT INTO Teams (TeamId, TournamentId, TeamName, TeamKey, Wallet, LogoUrl, CreatedBy, UpdatedBy, CreatedAt, UpdatedAt)
                    VALUES (@TeamId, @TournamentId, @TeamName, @TeamKey, @Wallet, @LogoUrl, @CreatedBy, @UpdatedBy, @CreatedAt, @UpdatedAt)";
        await connection.ExecuteAsync(sql, team);
        return team;
    }

    public async Task<Team> UpdateAsync(Team team)
    {
        using var connection = _context.CreateConnection();
        team.UpdatedAt = DateTime.UtcNow;
        var sql = @"UPDATE Teams SET TeamName = @TeamName, Wallet = @Wallet, LogoUrl = @LogoUrl, UpdatedBy = @UpdatedBy, UpdatedAt = @UpdatedAt
                    WHERE TeamId = @TeamId";
        await connection.ExecuteAsync(sql, team);
        return team;
    }

    public async Task<bool> DeleteAsync(Guid teamId)
    {
        using var connection = _context.CreateConnection();
        var sql = "DELETE FROM Teams WHERE TeamId = @TeamId";
        var rowsAffected = await connection.ExecuteAsync(sql, new { TeamId = teamId });
        return rowsAffected > 0;
    }
}

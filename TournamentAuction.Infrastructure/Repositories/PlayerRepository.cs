using Dapper;
using TournamentAuction.Domain.Entities;
using TournamentAuction.Domain.Interfaces;
using TournamentAuction.Infrastructure.Data;

namespace TournamentAuction.Infrastructure.Repositories;

public class PlayerRepository : IPlayerRepository
{
    private readonly DapperContext _context;

    public PlayerRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Player>> GetByTournamentIdAsync(Guid tournamentId)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT * FROM Players WHERE TournamentId = @TournamentId";
        return await connection.QueryAsync<Player>(sql, new { TournamentId = tournamentId });
    }

    public async Task<IEnumerable<Player>> GetByTeamIdAsync(Guid teamId)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT * FROM Players WHERE AssignedTeamId = @TeamId";
        return await connection.QueryAsync<Player>(sql, new { TeamId = teamId });
    }

    public async Task<Player?> GetByIdAsync(Guid playerId)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT * FROM Players WHERE PlayerId = @PlayerId";
        return await connection.QueryFirstOrDefaultAsync<Player>(sql, new { PlayerId = playerId });
    }

    public async Task<Player> CreateAsync(Player player)
    {
        using var connection = _context.CreateConnection();
        if (player.PlayerId == Guid.Empty)
        {
            player.PlayerId = Guid.NewGuid();
        }
        player.CreatedAt = DateTime.UtcNow;
        player.UpdatedAt = DateTime.UtcNow;
        
        var sql = @"INSERT INTO Players (PlayerId, TournamentId, Name, Category, BasePrice, AssignedTeamId, CreatedAt, UpdatedAt)
                    VALUES (@PlayerId, @TournamentId, @Name, @Category, @BasePrice, @AssignedTeamId, @CreatedAt, @UpdatedAt)";
        await connection.ExecuteAsync(sql, player);
        return player;
    }

    public async Task<Player> UpdateAsync(Player player)
    {
        using var connection = _context.CreateConnection();
        player.UpdatedAt = DateTime.UtcNow;
        var sql = @"UPDATE Players SET Name = @Name, Category = @Category, BasePrice = @BasePrice, AssignedTeamId = @AssignedTeamId, UpdatedAt = @UpdatedAt
                    WHERE PlayerId = @PlayerId";
        await connection.ExecuteAsync(sql, player);
        return player;
    }

    public async Task<bool> AssignToTeamAsync(Guid playerId, Guid teamId, decimal soldPrice)
    {
        using var connection = _context.CreateConnection();
        var sql = @"UPDATE Players SET AssignedTeamId = @TeamId, UpdatedAt = @UpdatedAt
                    WHERE PlayerId = @PlayerId";
        var rowsAffected = await connection.ExecuteAsync(sql, new { PlayerId = playerId, TeamId = teamId, UpdatedAt = DateTime.UtcNow });
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(Guid playerId)
    {
        using var connection = _context.CreateConnection();
        var sql = "DELETE FROM Players WHERE PlayerId = @PlayerId";
        var rowsAffected = await connection.ExecuteAsync(sql, new { PlayerId = playerId });
        return rowsAffected > 0;
    }
}

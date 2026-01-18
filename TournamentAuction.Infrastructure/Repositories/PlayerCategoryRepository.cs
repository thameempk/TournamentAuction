using Dapper;
using TournamentAuction.Domain.Entities;
using TournamentAuction.Domain.Interfaces;
using TournamentAuction.Infrastructure.Data;

namespace TournamentAuction.Infrastructure.Repositories;

public class PlayerCategoryRepository : IPlayerCategoryRepository
{
    private readonly DapperContext _context;

    public PlayerCategoryRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PlayerCategory>> GetByTournamentIdAsync(Guid tournamentId)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT * FROM PlayersCategory WHERE TournamentId = @TournamentId";
        return await connection.QueryAsync<PlayerCategory>(sql, new { TournamentId = tournamentId });
    }

    public async Task<PlayerCategory?> GetByIdAsync(Guid categoryId)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT * FROM PlayersCategory WHERE CategoryId = @CategoryId";
        return await connection.QueryFirstOrDefaultAsync<PlayerCategory>(sql, new { CategoryId = categoryId });
    }

    public async Task<PlayerCategory> CreateAsync(PlayerCategory category)
    {
        using var connection = _context.CreateConnection();
        if (category.CategoryId == Guid.Empty)
        {
            category.CategoryId = Guid.NewGuid();
        }
        category.CreatedAt = DateTime.UtcNow;
        category.UpdatedAt = DateTime.UtcNow;
        
        var sql = @"INSERT INTO PlayersCategory (CategoryId, TournamentId, CategoryName, CreatedBy, UpdatedBy, CreatedAt, UpdatedAt)
                    VALUES (@CategoryId, @TournamentId, @CategoryName, @CreatedBy, @UpdatedBy, @CreatedAt, @UpdatedAt)";
        await connection.ExecuteAsync(sql, category);
        return category;
    }

    public async Task<PlayerCategory> UpdateAsync(PlayerCategory category)
    {
        using var connection = _context.CreateConnection();
        category.UpdatedAt = DateTime.UtcNow;
        var sql = @"UPDATE PlayersCategory SET CategoryName = @CategoryName, UpdatedBy = @UpdatedBy, UpdatedAt = @UpdatedAt
                    WHERE CategoryId = @CategoryId";
        await connection.ExecuteAsync(sql, category);
        return category;
    }

    public async Task<bool> DeleteAsync(Guid categoryId)
    {
        using var connection = _context.CreateConnection();
        var sql = "DELETE FROM PlayersCategory WHERE CategoryId = @CategoryId";
        var rowsAffected = await connection.ExecuteAsync(sql, new { CategoryId = categoryId });
        return rowsAffected > 0;
    }
}

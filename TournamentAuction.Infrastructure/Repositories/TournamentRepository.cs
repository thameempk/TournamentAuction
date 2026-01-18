using Dapper;
using TournamentAuction.Domain.Entities;
using TournamentAuction.Domain.Interfaces;
using TournamentAuction.Infrastructure.Data;

namespace TournamentAuction.Infrastructure.Repositories;

public class TournamentRepository : ITournamentRepository
{
    private readonly DapperContext _context;

    public TournamentRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Tournament>> GetAllAsync()
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT * FROM Tournaments ORDER BY CreatedAt DESC";
        return await connection.QueryAsync<Tournament>(sql);
    }

    public async Task<Tournament?> GetByIdAsync(Guid tournamentId)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT * FROM Tournaments WHERE TournamentId = @TournamentId";
        return await connection.QueryFirstOrDefaultAsync<Tournament>(sql, new { TournamentId = tournamentId });
    }

    public async Task<Tournament> CreateAsync(Tournament tournament)
    {
        using var connection = _context.CreateConnection();
        if (tournament.TournamentId == Guid.Empty)
        {
            tournament.TournamentId = Guid.NewGuid();
        }
        tournament.CreatedAt = DateTime.UtcNow;
        tournament.UpdatedAt = DateTime.UtcNow;
        
        var sql = @"INSERT INTO Tournaments (TournamentId, Name, TournamentAdminId, Type, Format, AuctionEnabled, 
                    MinPlayers, MaxPlayers, MinBidIncrement, MaxTeamBudget, BidSeconds, Status, CreatedBy, UpdatedBy, CreatedAt, UpdatedAt)
                    VALUES (@TournamentId, @Name, @TournamentAdminId, @Type, @Format, @AuctionEnabled, 
                    @MinPlayers, @MaxPlayers, @MinBidIncrement, @MaxTeamBudget, @BidSeconds, @Status, @CreatedBy, @UpdatedBy, @CreatedAt, @UpdatedAt)";
        await connection.ExecuteAsync(sql, tournament);
        return tournament;
    }

    public async Task<Tournament> UpdateAsync(Tournament tournament)
    {
        using var connection = _context.CreateConnection();
        tournament.UpdatedAt = DateTime.UtcNow;
        var sql = @"UPDATE Tournaments SET Name = @Name, TournamentAdminId = @TournamentAdminId, Type = @Type, Format = @Format,
                    AuctionEnabled = @AuctionEnabled, MinPlayers = @MinPlayers, MaxPlayers = @MaxPlayers, 
                    MinBidIncrement = @MinBidIncrement, MaxTeamBudget = @MaxTeamBudget, BidSeconds = @BidSeconds,
                    Status = @Status, UpdatedBy = @UpdatedBy, UpdatedAt = @UpdatedAt
                    WHERE TournamentId = @TournamentId";
        await connection.ExecuteAsync(sql, tournament);
        return tournament;
    }

    public async Task<bool> DeleteAsync(Guid tournamentId)
    {
        using var connection = _context.CreateConnection();
        var sql = "DELETE FROM Tournaments WHERE TournamentId = @TournamentId";
        var rowsAffected = await connection.ExecuteAsync(sql, new { TournamentId = tournamentId });
        return rowsAffected > 0;
    }
}

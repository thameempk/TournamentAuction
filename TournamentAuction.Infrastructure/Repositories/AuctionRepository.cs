using Dapper;
using TournamentAuction.Domain.Entities;
using TournamentAuction.Domain.Interfaces;
using TournamentAuction.Infrastructure.Data;

namespace TournamentAuction.Infrastructure.Repositories;

public class AuctionRepository : IAuctionRepository
{
    private readonly DapperContext _context;

    public AuctionRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<Auction?> GetActiveAuctionByTournamentIdAsync(Guid tournamentId)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT * FROM Auction WHERE TournamentId = @TournamentId AND Status IN ('InProgress', 'Paused')";
        return await connection.QueryFirstOrDefaultAsync<Auction>(sql, new { TournamentId = tournamentId });
    }

    public async Task<Auction?> GetByIdAsync(Guid auctionId)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT * FROM Auction WHERE AuctionId = @AuctionId";
        return await connection.QueryFirstOrDefaultAsync<Auction>(sql, new { AuctionId = auctionId });
    }

    public async Task<Auction> CreateAsync(Auction auction)
    {
        using var connection = _context.CreateConnection();
        if (auction.AuctionId == Guid.Empty)
        {
            auction.AuctionId = Guid.NewGuid();
        }
        auction.CreatedAt = DateTime.UtcNow;
        auction.UpdatedAt = DateTime.UtcNow;
        
        var sql = @"INSERT INTO Auction (AuctionId, TournamentId, Status, StartedAt, EndedAt, CreatedBy, UpdatedBy, CreatedAt, UpdatedAt)
                    VALUES (@AuctionId, @TournamentId, @Status, @StartedAt, @EndedAt, @CreatedBy, @UpdatedBy, @CreatedAt, @UpdatedAt)";
        await connection.ExecuteAsync(sql, auction);
        return auction;
    }

    public async Task<Auction> UpdateAsync(Auction auction)
    {
        using var connection = _context.CreateConnection();
        auction.UpdatedAt = DateTime.UtcNow;
        var sql = @"UPDATE Auction SET Status = @Status, StartedAt = @StartedAt, EndedAt = @EndedAt, UpdatedBy = @UpdatedBy, UpdatedAt = @UpdatedAt
                    WHERE AuctionId = @AuctionId";
        await connection.ExecuteAsync(sql, auction);
        return auction;
    }

    public async Task<IEnumerable<Auction>> GetByTournamentIdAsync(Guid tournamentId)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT * FROM Auction WHERE TournamentId = @TournamentId ORDER BY CreatedAt DESC";
        return await connection.QueryAsync<Auction>(sql, new { TournamentId = tournamentId });
    }
}

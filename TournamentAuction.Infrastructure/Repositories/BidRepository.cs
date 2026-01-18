using Dapper;
using TournamentAuction.Domain.Entities;
using TournamentAuction.Domain.Interfaces;
using TournamentAuction.Infrastructure.Data;

namespace TournamentAuction.Infrastructure.Repositories;

public class BidRepository : IBidRepository
{
    private readonly DapperContext _context;

    public BidRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Bid>> GetByAuctionIdAsync(Guid auctionId)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT * FROM Bids WHERE AuctionId = @AuctionId ORDER BY BidAmount DESC";
        return await connection.QueryAsync<Bid>(sql, new { AuctionId = auctionId });
    }

    public async Task<Bid?> GetHighestBidByAuctionIdAsync(Guid auctionId)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT TOP 1 * FROM Bids WHERE AuctionId = @AuctionId ORDER BY BidAmount DESC";
        return await connection.QueryFirstOrDefaultAsync<Bid>(sql, new { AuctionId = auctionId });
    }

    public async Task<Bid> CreateAsync(Bid bid)
    {
        using var connection = _context.CreateConnection();
        if (bid.BidId == Guid.Empty)
        {
            bid.BidId = Guid.NewGuid();
        }
        
        var sql = @"INSERT INTO Bids (BidId, AuctionId, PlayerId, TeamId, BidAmount, BidTime)
                    VALUES (@BidId, @AuctionId, @PlayerId, @TeamId, @BidAmount, @BidTime)";
        await connection.ExecuteAsync(sql, bid);
        return bid;
    }

    public async Task<bool> UpdateWinningBidsAsync(Guid auctionId, Guid winningBidId)
    {
        using var connection = _context.CreateConnection();
        // Note: Bids table doesn't have IsWinningBid column in your schema
        // The winning bid is determined by highest bid amount
        return true;
    }
}

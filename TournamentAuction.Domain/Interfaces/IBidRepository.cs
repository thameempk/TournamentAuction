using TournamentAuction.Domain.Entities;

namespace TournamentAuction.Domain.Interfaces;

public interface IBidRepository
{
    Task<IEnumerable<Bid>> GetByAuctionIdAsync(Guid auctionId);
    Task<Bid?> GetHighestBidByAuctionIdAsync(Guid auctionId);
    Task<Bid> CreateAsync(Bid bid);
    Task<bool> UpdateWinningBidsAsync(Guid auctionId, Guid winningBidId);
}

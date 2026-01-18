using TournamentAuction.Domain.Entities;

namespace TournamentAuction.Domain.Interfaces;

public interface IAuctionRepository
{
    Task<Auction?> GetActiveAuctionByTournamentIdAsync(Guid tournamentId);
    Task<Auction?> GetByIdAsync(Guid auctionId);
    Task<Auction> CreateAsync(Auction auction);
    Task<Auction> UpdateAsync(Auction auction);
    Task<IEnumerable<Auction>> GetByTournamentIdAsync(Guid tournamentId);
}

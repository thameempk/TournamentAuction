using TournamentAuction.Domain.Entities;

namespace TournamentAuction.Domain.Interfaces;

public interface ITournamentRepository
{
    Task<IEnumerable<Tournament>> GetAllAsync();
    Task<Tournament?> GetByIdAsync(Guid tournamentId);
    Task<Tournament> CreateAsync(Tournament tournament);
    Task<Tournament> UpdateAsync(Tournament tournament);
    Task<bool> DeleteAsync(Guid tournamentId);
}

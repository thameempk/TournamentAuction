using TournamentAuction.Domain.Entities;

namespace TournamentAuction.Domain.Interfaces;

public interface IPlayerCategoryRepository
{
    Task<IEnumerable<PlayerCategory>> GetByTournamentIdAsync(Guid tournamentId);
    Task<PlayerCategory?> GetByIdAsync(Guid categoryId);
    Task<PlayerCategory> CreateAsync(PlayerCategory category);
    Task<PlayerCategory> UpdateAsync(PlayerCategory category);
    Task<bool> DeleteAsync(Guid categoryId);
}

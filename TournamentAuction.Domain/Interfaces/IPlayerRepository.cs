using TournamentAuction.Domain.Entities;

namespace TournamentAuction.Domain.Interfaces;

public interface IPlayerRepository
{
    Task<IEnumerable<Player>> GetByTournamentIdAsync(Guid tournamentId);
    Task<IEnumerable<Player>> GetByTeamIdAsync(Guid teamId);
    Task<Player?> GetByIdAsync(Guid playerId);
    Task<Player> CreateAsync(Player player);
    Task<Player> UpdateAsync(Player player);
    Task<bool> AssignToTeamAsync(Guid playerId, Guid teamId, decimal soldPrice);
    Task<bool> DeleteAsync(Guid playerId);
}

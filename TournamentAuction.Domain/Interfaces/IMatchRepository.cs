using TournamentAuction.Domain.Entities;

namespace TournamentAuction.Domain.Interfaces;

public interface IMatchRepository
{
    Task<IEnumerable<Match>> GetByTournamentIdAsync(Guid tournamentId);
    Task<Match?> GetByIdAsync(Guid matchId);
    Task<Match> CreateAsync(Match match);
    Task<Match> UpdateAsync(Match match);
    Task<bool> DeleteAsync(Guid matchId);
}

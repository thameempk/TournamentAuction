using TournamentAuction.Domain.Entities;

namespace TournamentAuction.Domain.Interfaces;

public interface ITeamRepository
{
    Task<IEnumerable<Team>> GetByTournamentIdAsync(Guid tournamentId);
    Task<Team?> GetByIdAsync(Guid teamId);
    Task<Team?> GetByTeamKeyAsync(string teamKey);
    Task<Team> CreateAsync(Team team);
    Task<Team> UpdateAsync(Team team);
    Task<bool> DeleteAsync(Guid teamId);
}

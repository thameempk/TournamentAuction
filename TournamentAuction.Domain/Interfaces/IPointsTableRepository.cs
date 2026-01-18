using TournamentAuction.Domain.Entities;

namespace TournamentAuction.Domain.Interfaces;

public interface IPointsTableRepository
{
    Task<IEnumerable<PointsTable>> GetByTournamentIdAsync(Guid tournamentId);
    Task<PointsTable?> GetByTournamentAndTeamIdAsync(Guid tournamentId, Guid teamId);
    Task<PointsTable> CreateOrUpdateAsync(PointsTable pointsTable);
    Task<bool> UpdateMatchResultAsync(Guid tournamentId, Guid team1Id, Guid team2Id, Guid? winnerId);
}

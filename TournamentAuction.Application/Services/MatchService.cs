using TournamentAuction.Domain.Entities;
using TournamentAuction.Domain.Interfaces;

namespace TournamentAuction.Application.Services;

public class MatchService
{
    private readonly IMatchRepository _matchRepository;
    private readonly IPointsTableRepository _pointsTableRepository;
    private readonly ITournamentRepository _tournamentRepository;
    private readonly ITeamRepository _teamRepository;

    public MatchService(
        IMatchRepository matchRepository,
        IPointsTableRepository pointsTableRepository,
        ITournamentRepository tournamentRepository,
        ITeamRepository teamRepository)
    {
        _matchRepository = matchRepository;
        _pointsTableRepository = pointsTableRepository;
        _tournamentRepository = tournamentRepository;
        _teamRepository = teamRepository;
    }

    public async Task<Match> ScheduleMatchAsync(Match match, Guid userId)
    {
        var tournament = await _tournamentRepository.GetByIdAsync(match.TournamentId);
        if (tournament == null)
            throw new ArgumentException("Tournament not found");

        if (match.TeamAId == match.TeamBId)
            throw new ArgumentException("A team cannot play against itself");

        if (match.MatchId == Guid.Empty)
        {
            match.MatchId = Guid.NewGuid();
        }
        match.Status = "Pending";
        match.CreatedBy = userId;
        match.UpdatedBy = userId;
        match.CreatedAt = DateTime.UtcNow;
        match.UpdatedAt = DateTime.UtcNow;
        
        return await _matchRepository.CreateAsync(match);
    }

    public async Task<List<Match>> AutoScheduleLeagueMatchesAsync(Guid tournamentId, Guid userId)
    {
        var tournament = await _tournamentRepository.GetByIdAsync(tournamentId);
        if (tournament == null || tournament.Format != "League")
            throw new ArgumentException("Tournament not found or not a league format");

        var teams = (await _teamRepository.GetByTournamentIdAsync(tournamentId)).ToList();
        if (teams.Count < 2)
            throw new InvalidOperationException("Need at least 2 teams for a league");

        var matches = new List<Match>();
        var scheduledDate = DateTime.UtcNow.AddDays(1);

        // Create round-robin matches
        for (int i = 0; i < teams.Count; i++)
        {
            for (int j = i + 1; j < teams.Count; j++)
            {
                var match = new Match
                {
                    MatchId = Guid.NewGuid(),
                    TournamentId = tournamentId,
                    TeamAId = teams[i].TeamId,
                    TeamBId = teams[j].TeamId,
                    ScheduledAt = scheduledDate,
                    Status = "Pending",
                    CreatedBy = userId,
                    UpdatedBy = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _matchRepository.CreateAsync(match);
                matches.Add(match);
                scheduledDate = scheduledDate.AddDays(1);
            }
        }

        return matches;
    }

    public async Task<Match> UpdateMatchResultAsync(Guid matchId, Guid? winnerId, int? teamAScore, int? teamBScore, Guid userId)
    {
        var match = await _matchRepository.GetByIdAsync(matchId);
        if (match == null)
            throw new ArgumentException("Match not found");

        var tournament = await _tournamentRepository.GetByIdAsync(match.TournamentId);
        if (tournament == null)
            throw new ArgumentException("Tournament not found");

        match.WinningTeamId = winnerId;
        match.Status = "Completed";
        match.UpdatedBy = userId;
        match.UpdatedAt = DateTime.UtcNow;
        await _matchRepository.UpdateAsync(match);

        // Update points table
        await UpdatePointsTableAsync(match.TournamentId, match.TeamAId, match.TeamBId, winnerId, teamAScore, teamBScore, tournament.Type);

        return match;
    }

    private async Task UpdatePointsTableAsync(Guid tournamentId, Guid teamAId, Guid teamBId, Guid? winnerId, int? teamAScore, int? teamBScore, Guid tournamentType)
    {
        var pointsTableA = await _pointsTableRepository.GetByTournamentAndTeamIdAsync(tournamentId, teamAId);
        var pointsTableB = await _pointsTableRepository.GetByTournamentAndTeamIdAsync(tournamentId, teamBId);

        var systemUserId = Guid.Empty; // System user

        if (pointsTableA == null)
        {
            pointsTableA = new PointsTable
            {
                PointId = Guid.NewGuid(),
                TournamentId = tournamentId,
                TeamId = teamAId,
                Points = 0,
                Wins = 0,
                Losses = 0,
                Draws = 0,
                GoalsScored = 0,
                GoalsConceded = 0,
                CreatedBy = systemUserId,
                UpdatedBy = systemUserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        if (pointsTableB == null)
        {
            pointsTableB = new PointsTable
            {
                PointId = Guid.NewGuid(),
                TournamentId = tournamentId,
                TeamId = teamBId,
                Points = 0,
                Wins = 0,
                Losses = 0,
                Draws = 0,
                GoalsScored = 0,
                GoalsConceded = 0,
                CreatedBy = systemUserId,
                UpdatedBy = systemUserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        // Update goals/scoring
        if (teamAScore.HasValue && teamBScore.HasValue)
        {
            pointsTableA.GoalsScored += teamAScore.Value;
            pointsTableA.GoalsConceded += teamBScore.Value;
            pointsTableB.GoalsScored += teamBScore.Value;
            pointsTableB.GoalsConceded += teamAScore.Value;

            // Calculate NRR for cricket or goal difference for football
            // Note: tournamentType is now a GUID, you may need to look up the type name
            // For now, using goal difference for both
            pointsTableA.GoalsOrNRR = teamAScore.Value - teamBScore.Value;
            pointsTableB.GoalsOrNRR = teamBScore.Value - teamAScore.Value;
        }

        // Update match results
        if (winnerId == teamAId)
        {
            // Team A wins
            pointsTableA.Wins++;
            pointsTableA.Points += 3; // Default to football scoring, adjust as needed
            pointsTableB.Losses++;
        }
        else if (winnerId == teamBId)
        {
            // Team B wins
            pointsTableB.Wins++;
            pointsTableB.Points += 3; // Default to football scoring, adjust as needed
            pointsTableA.Losses++;
        }
        else
        {
            // Draw
            pointsTableA.Draws++;
            pointsTableA.Points += 1;
            pointsTableB.Draws++;
            pointsTableB.Points += 1;
        }

        pointsTableA.UpdatedAt = DateTime.UtcNow;
        pointsTableB.UpdatedAt = DateTime.UtcNow;
        pointsTableA.UpdatedBy = systemUserId;
        pointsTableB.UpdatedBy = systemUserId;

        await _pointsTableRepository.CreateOrUpdateAsync(pointsTableA);
        await _pointsTableRepository.CreateOrUpdateAsync(pointsTableB);
    }

    public async Task<IEnumerable<PointsTable>> GetPointsTableAsync(Guid tournamentId)
    {
        var pointsTable = await _pointsTableRepository.GetByTournamentIdAsync(tournamentId);
        var tournament = await _tournamentRepository.GetByIdAsync(tournamentId);
        
        // Note: tournament.Type is now a GUID, you may need to look up the type
        // For now, sorting by points and goal difference/NRR
        return pointsTable.OrderByDescending(pt => pt.Points)
                         .ThenByDescending(pt => pt.GoalsOrNRR ?? 0)
                         .ThenByDescending(pt => pt.GoalsScored);
    }

    public async Task<List<Team>> DetermineWinnersAsync(Guid tournamentId)
    {
        var tournament = await _tournamentRepository.GetByIdAsync(tournamentId);
        if (tournament == null)
            throw new ArgumentException("Tournament not found");

        var pointsTable = (await GetPointsTableAsync(tournamentId)).ToList();
        var teams = (await _teamRepository.GetByTournamentIdAsync(tournamentId)).ToList();

        if (tournament.Format.ToLower() == "knockout")
        {
            // For knockout, winner is the team that won the final match
            var finalMatch = (await _matchRepository.GetByTournamentIdAsync(tournamentId))
                .Where(m => m.Status == "Completed")
                .OrderByDescending(m => m.ScheduledAt)
                .FirstOrDefault();

            if (finalMatch?.WinningTeamId != null)
            {
                var winner = teams.FirstOrDefault(t => t.TeamId == finalMatch.WinningTeamId);
                return winner != null ? new List<Team> { winner } : new List<Team>();
            }
        }
        else
        {
            // League or Mixed - top team(s) based on points table
            var topTeam = pointsTable.FirstOrDefault();
            if (topTeam != null)
            {
                var winner = teams.FirstOrDefault(t => t.TeamId == topTeam.TeamId);
                return winner != null ? new List<Team> { winner } : new List<Team>();
            }
        }

        return new List<Team>();
    }
}

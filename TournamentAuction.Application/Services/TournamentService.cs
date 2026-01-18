using TournamentAuction.Domain.Entities;
using TournamentAuction.Domain.Interfaces;

namespace TournamentAuction.Application.Services;

public class TournamentService
{
    private readonly ITournamentRepository _tournamentRepository;
    private readonly ITeamRepository _teamRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly IPlayerCategoryRepository _playerCategoryRepository;

    public TournamentService(
        ITournamentRepository tournamentRepository,
        ITeamRepository teamRepository,
        IPlayerRepository playerRepository,
        IPlayerCategoryRepository playerCategoryRepository)
    {
        _tournamentRepository = tournamentRepository;
        _teamRepository = teamRepository;
        _playerRepository = playerRepository;
        _playerCategoryRepository = playerCategoryRepository;
    }

    public async Task<Tournament> CreateTournamentAsync(Tournament tournament, Guid createdBy)
    {
        ValidateTournamentRules(tournament);
        
        if (tournament.TournamentId == Guid.Empty)
        {
            tournament.TournamentId = Guid.NewGuid();
        }
        tournament.Status = "Draft";
        tournament.CreatedBy = createdBy;
        tournament.UpdatedBy = createdBy;
        tournament.CreatedAt = DateTime.UtcNow;
        tournament.UpdatedAt = DateTime.UtcNow;
        
        return await _tournamentRepository.CreateAsync(tournament);
    }

    public async Task<Tournament> UpdateTournamentRulesAsync(Tournament tournament, Guid updatedBy)
    {
        ValidateTournamentRules(tournament);
        
        tournament.UpdatedBy = updatedBy;
        tournament.UpdatedAt = DateTime.UtcNow;
        
        // If all rules are configured, move to Configured status
        if (tournament.Status == "Draft" && IsFullyConfigured(tournament))
        {
            tournament.Status = "Configured";
        }
        
        return await _tournamentRepository.UpdateAsync(tournament);
    }

    private void ValidateTournamentRules(Tournament tournament)
    {
        if (tournament.MinPlayers < 1)
            throw new ArgumentException("Minimum players must be at least 1");

        if (tournament.MaxPlayers < tournament.MinPlayers)
            throw new ArgumentException("Maximum players must be greater than or equal to minimum players");

        if (tournament.AuctionEnabled)
        {
            if (!tournament.MinBidIncrement.HasValue || tournament.MinBidIncrement <= 0)
                throw new ArgumentException("Minimum bid increment is required when auction is enabled");

            if (!tournament.MaxTeamBudget.HasValue || tournament.MaxTeamBudget <= 0)
                throw new ArgumentException("Maximum team budget is required when auction is enabled");

            if (!tournament.BidSeconds.HasValue || tournament.BidSeconds <= 0)
                throw new ArgumentException("Bid duration in seconds is required when auction is enabled");
        }
    }

    private bool IsFullyConfigured(Tournament tournament)
    {
        if (string.IsNullOrEmpty(tournament.Type.ToString()) || string.IsNullOrEmpty(tournament.Format))
            return false;

        if (tournament.AuctionEnabled)
        {
            return tournament.MinBidIncrement.HasValue &&
                   tournament.MaxTeamBudget.HasValue &&
                   tournament.BidSeconds.HasValue;
        }

        return true;
    }

    public async Task<bool> CanStartTournamentAsync(Guid tournamentId)
    {
        var tournament = await _tournamentRepository.GetByIdAsync(tournamentId);
        if (tournament == null || tournament.Status != "Configured") return false;

        var teams = await _teamRepository.GetByTournamentIdAsync(tournamentId);
        var players = await _playerRepository.GetByTournamentIdAsync(tournamentId);

        // Check if we have enough teams (at least 2)
        if (teams.Count() < 2) return false;

        // Check if we have enough players
        var requiredPlayers = teams.Count() * tournament.MinPlayers;
        if (players.Count() < requiredPlayers) return false;

        return true;
    }

    public async Task<Tournament> StartTournamentAsync(Guid tournamentId, Guid userId)
    {
        var tournament = await _tournamentRepository.GetByIdAsync(tournamentId);
        if (tournament == null)
            throw new ArgumentException("Tournament not found");

        if (!await CanStartTournamentAsync(tournamentId))
            throw new InvalidOperationException("Tournament cannot be started. Check teams and players.");

        tournament.Status = "InProgress";
        tournament.UpdatedBy = userId;
        tournament.UpdatedAt = DateTime.UtcNow;

        return await _tournamentRepository.UpdateAsync(tournament);
    }

    public async Task<Tournament> CompleteTournamentAsync(Guid tournamentId, Guid userId)
    {
        var tournament = await _tournamentRepository.GetByIdAsync(tournamentId);
        if (tournament == null)
            throw new ArgumentException("Tournament not found");

        tournament.Status = "Completed";
        tournament.UpdatedBy = userId;
        tournament.UpdatedAt = DateTime.UtcNow;

        return await _tournamentRepository.UpdateAsync(tournament);
    }
}

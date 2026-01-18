using TournamentAuction.Domain.Entities;
using TournamentAuction.Domain.Interfaces;

namespace TournamentAuction.Application.Services;

public class AuctionService
{
    private readonly IAuctionRepository _auctionRepository;
    private readonly IBidRepository _bidRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly ITeamRepository _teamRepository;
    private readonly ITournamentRepository _tournamentRepository;

    public AuctionService(
        IAuctionRepository auctionRepository,
        IBidRepository bidRepository,
        IPlayerRepository playerRepository,
        ITeamRepository teamRepository,
        ITournamentRepository tournamentRepository)
    {
        _auctionRepository = auctionRepository;
        _bidRepository = bidRepository;
        _playerRepository = playerRepository;
        _teamRepository = teamRepository;
        _tournamentRepository = tournamentRepository;
    }

    public async Task<Auction> StartAuctionAsync(Guid tournamentId, Guid userId)
    {
        var tournament = await _tournamentRepository.GetByIdAsync(tournamentId);
        if (tournament == null)
            throw new ArgumentException("Tournament not found");

        if (!tournament.AuctionEnabled)
            throw new InvalidOperationException("Auction is not enabled for this tournament");

        var existingAuction = await _auctionRepository.GetActiveAuctionByTournamentIdAsync(tournamentId);
        if (existingAuction != null && existingAuction.Status == "InProgress")
            throw new InvalidOperationException("An auction is already in progress");

        // Initialize team wallets if not already set
        var teams = await _teamRepository.GetByTournamentIdAsync(tournamentId);
        foreach (var team in teams)
        {
            if (!team.Wallet.HasValue && tournament.MaxTeamBudget.HasValue)
            {
                team.Wallet = tournament.MaxTeamBudget.Value;
                await _teamRepository.UpdateAsync(team);
            }
        }

        var auction = new Auction
        {
            AuctionId = Guid.NewGuid(),
            TournamentId = tournamentId,
            Status = "InProgress",
            StartedAt = DateTime.UtcNow,
            CreatedBy = userId,
            UpdatedBy = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return await _auctionRepository.CreateAsync(auction);
    }

    public async Task<Bid> PlaceBidAsync(Guid auctionId, Guid playerId, Guid teamId, decimal bidAmount)
    {
        var auction = await _auctionRepository.GetByIdAsync(auctionId);
        if (auction == null || auction.Status != "InProgress")
            throw new InvalidOperationException("Auction is not active");

        var tournament = await _tournamentRepository.GetByIdAsync(auction.TournamentId);
        if (tournament == null)
            throw new ArgumentException("Tournament not found");

        var player = await _playerRepository.GetByIdAsync(playerId);
        if (player == null)
            throw new ArgumentException("Player not found");

        if (player.AssignedTeamId != null)
            throw new InvalidOperationException("Player is already assigned");

        var team = await _teamRepository.GetByIdAsync(teamId);
        if (team == null)
            throw new ArgumentException("Team not found");

        // Get current highest bid
        var highestBid = await _bidRepository.GetHighestBidByAuctionIdAsync(auctionId);
        var currentBid = highestBid?.BidAmount ?? player.BasePrice;

        // Validate bid amount
        if (bidAmount <= currentBid)
            throw new ArgumentException($"Bid must be higher than current bid of {currentBid}");

        if (tournament.MinBidIncrement.HasValue && bidAmount < currentBid + tournament.MinBidIncrement.Value)
            throw new ArgumentException($"Bid must be at least {tournament.MinBidIncrement.Value} more than current bid");

        if (!team.Wallet.HasValue || team.Wallet.Value < bidAmount)
            throw new InvalidOperationException("Insufficient wallet balance");

        var bid = new Bid
        {
            BidId = Guid.NewGuid(),
            AuctionId = auctionId,
            PlayerId = playerId,
            TeamId = teamId,
            BidAmount = bidAmount,
            BidTime = DateTime.UtcNow
        };

        await _bidRepository.CreateAsync(bid);
        return bid;
    }

    public async Task<bool> AutoDistributePlayerAsync(Guid auctionId, Guid playerId)
    {
        var auction = await _auctionRepository.GetByIdAsync(auctionId);
        if (auction == null || auction.Status != "InProgress")
            return false;

        var player = await _playerRepository.GetByIdAsync(playerId);
        if (player == null || player.AssignedTeamId != null)
            return false;

        var highestBid = await _bidRepository.GetHighestBidByAuctionIdAsync(auctionId);
        if (highestBid == null)
        {
            // No bids - player remains unassigned for manual distribution
            return false;
        }

        // Assign player to winning team
        var team = await _teamRepository.GetByIdAsync(highestBid.TeamId);
        if (team == null || !team.Wallet.HasValue || team.Wallet.Value < highestBid.BidAmount)
            return false;

        // Deduct wallet
        team.Wallet = team.Wallet.Value - highestBid.BidAmount;
        await _teamRepository.UpdateAsync(team);

        // Assign player
        await _playerRepository.AssignToTeamAsync(playerId, highestBid.TeamId, highestBid.BidAmount);

        // Mark winning bid
        await _bidRepository.UpdateWinningBidsAsync(auctionId, highestBid.BidId);

        return true;
    }

    public async Task<List<Player>> AutoDistributeRemainingPlayersAsync(Guid tournamentId)
    {
        var tournament = await _tournamentRepository.GetByIdAsync(tournamentId);
        if (tournament == null)
            throw new ArgumentException("Tournament not found");

        var unassignedPlayers = (await _playerRepository.GetByTournamentIdAsync(tournamentId))
            .Where(p => p.AssignedTeamId == null)
            .ToList();

        var teams = (await _teamRepository.GetByTournamentIdAsync(tournamentId)).ToList();
        var distributed = new List<Player>();

        // Distribute players randomly to teams that need them
        var random = new Random();
        foreach (var player in unassignedPlayers)
        {
            // Find teams that need more players
            var teamsNeedingPlayers = teams.Where(t =>
            {
                var teamPlayers = _playerRepository.GetByTeamIdAsync(t.TeamId).Result.Count();
                return teamPlayers < tournament.MaxPlayers;
            }).ToList();

            if (teamsNeedingPlayers.Any())
            {
                var randomTeam = teamsNeedingPlayers[random.Next(teamsNeedingPlayers.Count)];
                await _playerRepository.AssignToTeamAsync(player.PlayerId, randomTeam.TeamId, player.BasePrice);
                distributed.Add(player);
            }
        }

        return distributed;
    }

    public async Task<bool> PauseAuctionAsync(Guid auctionId, Guid userId)
    {
        var auction = await _auctionRepository.GetByIdAsync(auctionId);
        if (auction == null || auction.Status != "InProgress")
            return false;

        auction.Status = "Paused";
        auction.UpdatedBy = userId;
        auction.UpdatedAt = DateTime.UtcNow;
        await _auctionRepository.UpdateAsync(auction);
        return true;
    }

    public async Task<bool> ResumeAuctionAsync(Guid auctionId, Guid userId)
    {
        var auction = await _auctionRepository.GetByIdAsync(auctionId);
        if (auction == null || auction.Status != "Paused")
            return false;

        auction.Status = "InProgress";
        auction.UpdatedBy = userId;
        auction.UpdatedAt = DateTime.UtcNow;
        await _auctionRepository.UpdateAsync(auction);
        return true;
    }

    public async Task<Auction> EndAuctionAsync(Guid auctionId, Guid userId)
    {
        var auction = await _auctionRepository.GetByIdAsync(auctionId);
        if (auction == null)
            throw new ArgumentException("Auction not found");

        auction.Status = "Completed";
        auction.EndedAt = DateTime.UtcNow;
        auction.UpdatedBy = userId;
        auction.UpdatedAt = DateTime.UtcNow;
        await _auctionRepository.UpdateAsync(auction);

        // Auto-distribute remaining unassigned players
        await AutoDistributeRemainingPlayersAsync(auction.TournamentId);

        return auction;
    }
}

using Microsoft.AspNetCore.SignalR;

namespace TournamentAuction.Infrastructure.Hubs;

public class AuctionHub : Hub
{
    public async Task StartAuction(int playerId, int tournamentId)
    {
        await Clients.All.SendAsync("AuctionStarted", playerId, tournamentId);
    }

    public async Task PlaceBid(int teamId, int playerId, decimal bidAmount, int auctionId)
    {
        await Clients.All.SendAsync("BidPlaced", teamId, playerId, bidAmount, auctionId);
    }

    public async Task PlayerAssigned(int playerId, int teamId, decimal soldPrice)
    {
        await Clients.All.SendAsync("PlayerAssigned", playerId, teamId, soldPrice);
    }

    public async Task UpdateTimer(int auctionId, int secondsRemaining)
    {
        await Clients.All.SendAsync("TimerUpdate", auctionId, secondsRemaining);
    }

    public async Task AuctionPaused(int auctionId)
    {
        await Clients.All.SendAsync("AuctionPaused", auctionId);
    }

    public async Task AuctionResumed(int auctionId)
    {
        await Clients.All.SendAsync("AuctionResumed", auctionId);
    }

    public async Task JoinTournamentGroup(int tournamentId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"tournament_{tournamentId}");
    }

    public async Task LeaveTournamentGroup(int tournamentId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"tournament_{tournamentId}");
    }
}


using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using TournamentAuction.Application.Services;
using TournamentAuction.Domain.Interfaces;
using TournamentAuction.Infrastructure.Hubs;

namespace TournamentAuction.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AuctionController : ControllerBase
{
    private readonly AuctionService _auctionService;
    private readonly IAuctionRepository _auctionRepository;
    private readonly IHubContext<AuctionHub> _hubContext;

    public AuctionController(
        AuctionService auctionService,
        IAuctionRepository auctionRepository,
        IHubContext<AuctionHub> hubContext)
    {
        _auctionService = auctionService;
        _auctionRepository = auctionRepository;
        _hubContext = hubContext;
    }

    [HttpPost("start")]
    [Authorize(Roles = "Admin,TournamentAdmin")]
    public async Task<IActionResult> StartAuction([FromBody] StartAuctionRequest request)
    {
        try
        {
            if (!Guid.TryParse(request.TournamentId, out var tournamentId))
                return BadRequest(new { message = "Invalid tournament ID format" });

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var auction = await _auctionService.StartAuctionAsync(tournamentId, userId);
            
            await _hubContext.Clients.All.SendAsync("AuctionStarted", request.TournamentId, auction.AuctionId.ToString());
            
            return Ok(auction);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("bid")]
    [Authorize(Roles = "Team")]
    public async Task<IActionResult> PlaceBid([FromBody] PlaceBidRequest request)
    {
        try
        {
            if (!Guid.TryParse(request.AuctionId, out var auctionId))
                return BadRequest(new { message = "Invalid auction ID format" });

            if (!Guid.TryParse(request.PlayerId, out var playerId))
                return BadRequest(new { message = "Invalid player ID format" });

            var teamIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (teamIdClaim == null || !Guid.TryParse(teamIdClaim, out var teamId))
                return Unauthorized();

            var bid = await _auctionService.PlaceBidAsync(auctionId, playerId, teamId, request.BidAmount);
            
            await _hubContext.Clients.All.SendAsync("BidPlaced", 
                teamId.ToString(), 
                request.PlayerId, 
                request.BidAmount, 
                request.AuctionId);
            
            return Ok(bid);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{auctionId}/auto-distribute")]
    [Authorize(Roles = "Admin,TournamentAdmin")]
    public async Task<IActionResult> AutoDistribute(string auctionId, [FromBody] AutoDistributeRequest request)
    {
        try
        {
            if (!Guid.TryParse(auctionId, out var auctionIdGuid))
                return BadRequest(new { message = "Invalid auction ID format" });

            if (!Guid.TryParse(request.PlayerId, out var playerId))
                return BadRequest(new { message = "Invalid player ID format" });

            var result = await _auctionService.AutoDistributePlayerAsync(auctionIdGuid, playerId);
            if (!result) return BadRequest(new { message = "Could not assign player" });

            var auction = await _auctionRepository.GetByIdAsync(auctionIdGuid);
            if (auction != null)
            {
                await _hubContext.Clients.All.SendAsync("PlayerAssigned", 
                    request.PlayerId, 
                    auction.TournamentId.ToString());
            }

            return Ok(new { message = "Player assigned successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{auctionId}/pause")]
    [Authorize(Roles = "Admin,TournamentAdmin")]
    public async Task<IActionResult> Pause(string auctionId)
    {
        try
        {
            if (!Guid.TryParse(auctionId, out var auctionIdGuid))
                return BadRequest(new { message = "Invalid auction ID format" });

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var result = await _auctionService.PauseAuctionAsync(auctionIdGuid, userId);
            if (!result) return BadRequest();
            
            await _hubContext.Clients.All.SendAsync("AuctionPaused", auctionId);
            return Ok(new { message = "Auction paused" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{auctionId}/resume")]
    [Authorize(Roles = "Admin,TournamentAdmin")]
    public async Task<IActionResult> Resume(string auctionId)
    {
        try
        {
            if (!Guid.TryParse(auctionId, out var auctionIdGuid))
                return BadRequest(new { message = "Invalid auction ID format" });

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var result = await _auctionService.ResumeAuctionAsync(auctionIdGuid, userId);
            if (!result) return BadRequest();
            
            await _hubContext.Clients.All.SendAsync("AuctionResumed", auctionId);
            return Ok(new { message = "Auction resumed" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{auctionId}/end")]
    [Authorize(Roles = "Admin,TournamentAdmin")]
    public async Task<IActionResult> EndAuction(string auctionId)
    {
        try
        {
            if (!Guid.TryParse(auctionId, out var auctionIdGuid))
                return BadRequest(new { message = "Invalid auction ID format" });

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var auction = await _auctionService.EndAuctionAsync(auctionIdGuid, userId);
            await _hubContext.Clients.All.SendAsync("AuctionEnded", auctionId);
            return Ok(new { message = "Auction ended and players distributed" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("tournament/{tournamentId}")]
    public async Task<IActionResult> GetByTournament(string tournamentId)
    {
        if (!Guid.TryParse(tournamentId, out var tournamentIdGuid))
            return BadRequest(new { message = "Invalid tournament ID format" });

        var auctions = await _auctionRepository.GetByTournamentIdAsync(tournamentIdGuid);
        return Ok(auctions);
    }

    [HttpGet("tournament/{tournamentId}/active")]
    public async Task<IActionResult> GetActive(string tournamentId)
    {
        if (!Guid.TryParse(tournamentId, out var tournamentIdGuid))
            return BadRequest(new { message = "Invalid tournament ID format" });

        var auction = await _auctionRepository.GetActiveAuctionByTournamentIdAsync(tournamentIdGuid);
        return Ok(auction);
    }
}

public class StartAuctionRequest
{
    public string TournamentId { get; set; } = string.Empty;
}

public class PlaceBidRequest
{
    public string AuctionId { get; set; } = string.Empty;
    public string PlayerId { get; set; } = string.Empty;
    public decimal BidAmount { get; set; }
}

public class AutoDistributeRequest
{
    public string PlayerId { get; set; } = string.Empty;
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TournamentAuction.Application.Services;
using TournamentAuction.Domain.Entities;
using TournamentAuction.Domain.Interfaces;

namespace TournamentAuction.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MatchController : ControllerBase
{
    private readonly MatchService _matchService;
    private readonly IMatchRepository _matchRepository;

    public MatchController(MatchService matchService, IMatchRepository matchRepository)
    {
        _matchService = matchService;
        _matchRepository = matchRepository;
    }

    [HttpGet("tournament/{tournamentId}")]
    public async Task<IActionResult> GetByTournament(string tournamentId)
    {
        if (!Guid.TryParse(tournamentId, out var tournamentIdGuid))
            return BadRequest(new { message = "Invalid tournament ID format" });

        var matches = await _matchRepository.GetByTournamentIdAsync(tournamentIdGuid);
        return Ok(matches);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        if (!Guid.TryParse(id, out var matchId))
            return BadRequest(new { message = "Invalid match ID format" });

        var match = await _matchRepository.GetByIdAsync(matchId);
        if (match == null) return NotFound();
        return Ok(match);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,TournamentAdmin")]
    public async Task<IActionResult> Schedule([FromBody] Match match)
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var scheduled = await _matchService.ScheduleMatchAsync(match, userId);
            return CreatedAtAction(nameof(GetById), new { id = scheduled.MatchId }, scheduled);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("tournament/{tournamentId}/auto-schedule")]
    [Authorize(Roles = "Admin,TournamentAdmin")]
    public async Task<IActionResult> AutoSchedule(string tournamentId)
    {
        try
        {
            if (!Guid.TryParse(tournamentId, out var tournamentIdGuid))
                return BadRequest(new { message = "Invalid tournament ID format" });

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var matches = await _matchService.AutoScheduleLeagueMatchesAsync(tournamentIdGuid, userId);
            return Ok(matches);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}/result")]
    [Authorize(Roles = "Admin,TournamentAdmin")]
    public async Task<IActionResult> UpdateResult(string id, [FromBody] UpdateMatchResultRequest request)
    {
        try
        {
            if (!Guid.TryParse(id, out var matchId))
                return BadRequest(new { message = "Invalid match ID format" });

            Guid? winnerId = null;
            if (!string.IsNullOrEmpty(request.WinnerId) && Guid.TryParse(request.WinnerId, out var winnerIdGuid))
            {
                winnerId = winnerIdGuid;
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var match = await _matchService.UpdateMatchResultAsync(matchId, winnerId, request.TeamAScore, request.TeamBScore, userId);
            return Ok(match);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,TournamentAdmin")]
    public async Task<IActionResult> Delete(string id)
    {
        if (!Guid.TryParse(id, out var matchId))
            return BadRequest(new { message = "Invalid match ID format" });

        var result = await _matchRepository.DeleteAsync(matchId);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpGet("tournament/{tournamentId}/points-table")]
    public async Task<IActionResult> GetPointsTable(string tournamentId)
    {
        if (!Guid.TryParse(tournamentId, out var tournamentIdGuid))
            return BadRequest(new { message = "Invalid tournament ID format" });

        var pointsTable = await _matchService.GetPointsTableAsync(tournamentIdGuid);
        return Ok(pointsTable);
    }

    [HttpGet("tournament/{tournamentId}/winners")]
    public async Task<IActionResult> GetWinners(string tournamentId)
    {
        try
        {
            if (!Guid.TryParse(tournamentId, out var tournamentIdGuid))
                return BadRequest(new { message = "Invalid tournament ID format" });

            var winners = await _matchService.DetermineWinnersAsync(tournamentIdGuid);
            return Ok(winners);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

public class UpdateMatchResultRequest
{
    public string? WinnerId { get; set; }
    public int? TeamAScore { get; set; }
    public int? TeamBScore { get; set; }
}

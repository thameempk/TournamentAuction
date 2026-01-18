using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Dapper;
using TournamentAuction.Application.Services;
using TournamentAuction.Domain.Entities;
using TournamentAuction.Domain.Interfaces;
using TournamentAuction.Infrastructure.Data;

namespace TournamentAuction.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TournamentController : ControllerBase
{
    private readonly ITournamentRepository _tournamentRepository;
    private readonly TournamentService _tournamentService;
    private readonly DapperContext _context;

    public TournamentController(ITournamentRepository tournamentRepository, TournamentService tournamentService, DapperContext context)
    {
        _tournamentRepository = tournamentRepository;
        _tournamentService = tournamentService;
        _context = context;
    }

    [HttpGet("types")]
    public async Task<IActionResult> GetTournamentTypes()
    {
        var query = @"
            SELECT TypeId, Name, CreatedBy, UpdatedBy, CreatedAt, UpdatedAt
            FROM TournamentsTypes
            ORDER BY Name";

        using var connection = _context.CreateConnection();
        var types = await connection.QueryAsync<TournamentType>(query);
        return Ok(types);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userType = User.FindFirstValue(ClaimTypes.Role);
        
        var tournaments = await _tournamentRepository.GetAllAsync();
        
        // TournamentAdmin can only see their tournaments
        if (userType == "TournamentAdmin")
        {
            tournaments = tournaments.Where(t => t.CreatedBy.ToString() == userId);
        }
        
        return Ok(tournaments);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        if (!Guid.TryParse(id, out var tournamentId))
            return BadRequest(new { message = "Invalid tournament ID format" });

        var tournament = await _tournamentRepository.GetByIdAsync(tournamentId);
        if (tournament == null) return NotFound();
        
        // Check access for TournamentAdmin
        var userType = User.FindFirstValue(ClaimTypes.Role);
        if (userType == "TournamentAdmin")
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && Guid.TryParse(userIdClaim, out var userId))
            {
                if (tournament.CreatedBy != userId)
                {
                    return Forbid();
                }
            }
        }
        
        return Ok(tournament);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,TournamentAdmin")]
    public async Task<IActionResult> Create([FromBody] Tournament tournament)
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var created = await _tournamentService.CreateTournamentAsync(tournament, userId);
            return CreatedAtAction(nameof(GetById), new { id = created.TournamentId }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,TournamentAdmin")]
    public async Task<IActionResult> Update(string id, [FromBody] Tournament tournament)
    {
        if (!Guid.TryParse(id, out var tournamentId) || tournament.TournamentId != tournamentId)
            return BadRequest();
        
        // Check access
        var existing = await _tournamentRepository.GetByIdAsync(tournamentId);
        if (existing == null) return NotFound();
        
        var userType = User.FindFirstValue(ClaimTypes.Role);
        if (userType == "TournamentAdmin")
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && Guid.TryParse(userIdClaim, out var userId))
            {
                if (existing.CreatedBy != userId)
                {
                    return Forbid();
                }
            }
        }
        
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var updated = await _tournamentService.UpdateTournamentRulesAsync(tournament, userId);
            return Ok(updated);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(string id)
    {
        if (!Guid.TryParse(id, out var tournamentId))
            return BadRequest(new { message = "Invalid tournament ID format" });

        var result = await _tournamentRepository.DeleteAsync(tournamentId);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpGet("{id}/can-start")]
    public async Task<IActionResult> CanStart(string id)
    {
        if (!Guid.TryParse(id, out var tournamentId))
            return BadRequest(new { message = "Invalid tournament ID format" });

        var canStart = await _tournamentService.CanStartTournamentAsync(tournamentId);
        return Ok(new { canStart });
    }

    [HttpPost("{id}/start")]
    [Authorize(Roles = "Admin,TournamentAdmin")]
    public async Task<IActionResult> Start(string id)
    {
        if (!Guid.TryParse(id, out var tournamentId))
            return BadRequest(new { message = "Invalid tournament ID format" });

        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var tournament = await _tournamentService.StartTournamentAsync(tournamentId, userId);
            return Ok(tournament);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/complete")]
    [Authorize(Roles = "Admin,TournamentAdmin")]
    public async Task<IActionResult> Complete(string id)
    {
        if (!Guid.TryParse(id, out var tournamentId))
            return BadRequest(new { message = "Invalid tournament ID format" });

        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var tournament = await _tournamentService.CompleteTournamentAsync(tournamentId, userId);
            return Ok(tournament);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

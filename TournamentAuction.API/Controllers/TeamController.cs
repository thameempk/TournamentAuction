using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TournamentAuction.Domain.Entities;
using TournamentAuction.Domain.Interfaces;

namespace TournamentAuction.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TeamController : ControllerBase
{
    private readonly ITeamRepository _teamRepository;
    private readonly IPointsTableRepository _pointsTableRepository;
    private readonly IPlayerRepository _playerRepository;

    public TeamController(
        ITeamRepository teamRepository,
        IPointsTableRepository pointsTableRepository,
        IPlayerRepository playerRepository)
    {
        _teamRepository = teamRepository;
        _pointsTableRepository = pointsTableRepository;
        _playerRepository = playerRepository;
    }

    [HttpGet("tournament/{tournamentId}")]
    public async Task<IActionResult> GetByTournament(string tournamentId)
    {
        if (!Guid.TryParse(tournamentId, out var tournamentIdGuid))
            return BadRequest(new { message = "Invalid tournament ID format" });

        var teams = await _teamRepository.GetByTournamentIdAsync(tournamentIdGuid);
        return Ok(teams);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        if (!Guid.TryParse(id, out var teamId))
            return BadRequest(new { message = "Invalid team ID format" });

        var team = await _teamRepository.GetByIdAsync(teamId);
        if (team == null) return NotFound();
        
        // Teams can only see their own data
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        if (userRole == "Team")
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId) || userId != teamId)
            {
                return Forbid();
            }
        }
        
        return Ok(team);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,TournamentAdmin")]
    public async Task<IActionResult> Create([FromBody] Team team)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        team.CreatedBy = userId;
        team.UpdatedBy = userId;
        
        var created = await _teamRepository.CreateAsync(team);
        return CreatedAtAction(nameof(GetById), new { id = created.TeamId }, created);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,TournamentAdmin")]
    public async Task<IActionResult> Update(string id, [FromBody] Team team)
    {
        if (!Guid.TryParse(id, out var teamId) || team.TeamId != teamId)
            return BadRequest();
        
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        team.UpdatedBy = userId;
        
        var updated = await _teamRepository.UpdateAsync(team);
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,TournamentAdmin")]
    public async Task<IActionResult> Delete(string id)
    {
        if (!Guid.TryParse(id, out var teamId))
            return BadRequest(new { message = "Invalid team ID format" });

        var result = await _teamRepository.DeleteAsync(teamId);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpGet("{id}/wallet")]
    public async Task<IActionResult> GetWallet(string id)
    {
        if (!Guid.TryParse(id, out var teamId))
            return BadRequest(new { message = "Invalid team ID format" });

        var team = await _teamRepository.GetByIdAsync(teamId);
        if (team == null) return NotFound();
        
        // Teams can only see their own wallet
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        if (userRole == "Team")
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId) || userId != teamId)
            {
                return Forbid();
            }
        }
        
        return Ok(new
        {
            wallet = team.Wallet ?? 0,
            teamName = team.TeamName
        });
    }

    [HttpGet("{id}/squad")]
    public async Task<IActionResult> GetSquad(string id)
    {
        if (!Guid.TryParse(id, out var teamId))
            return BadRequest(new { message = "Invalid team ID format" });

        var players = await _playerRepository.GetByTeamIdAsync(teamId);
        return Ok(players);
    }

    [HttpGet("{id}/points")]
    public async Task<IActionResult> GetPoints(string id)
    {
        if (!Guid.TryParse(id, out var teamId))
            return BadRequest(new { message = "Invalid team ID format" });

        var team = await _teamRepository.GetByIdAsync(teamId);
        if (team == null) return NotFound();
        
        var pointsTable = await _pointsTableRepository.GetByTournamentAndTeamIdAsync(team.TournamentId, teamId);
        return Ok(pointsTable);
    }
}

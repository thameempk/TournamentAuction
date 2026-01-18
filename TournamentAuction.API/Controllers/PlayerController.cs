using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TournamentAuction.Domain.Entities;
using TournamentAuction.Domain.Interfaces;

namespace TournamentAuction.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PlayerController : ControllerBase
{
    private readonly IPlayerRepository _playerRepository;

    public PlayerController(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository;
    }

    [HttpGet("tournament/{tournamentId}")]
    public async Task<IActionResult> GetByTournament(string tournamentId)
    {
        if (!Guid.TryParse(tournamentId, out var tournamentIdGuid))
            return BadRequest(new { message = "Invalid tournament ID format" });

        var players = await _playerRepository.GetByTournamentIdAsync(tournamentIdGuid);
        return Ok(players);
    }

    [HttpGet("team/{teamId}")]
    public async Task<IActionResult> GetByTeam(string teamId)
    {
        if (!Guid.TryParse(teamId, out var teamIdGuid))
            return BadRequest(new { message = "Invalid team ID format" });

        var players = await _playerRepository.GetByTeamIdAsync(teamIdGuid);
        return Ok(players);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        if (!Guid.TryParse(id, out var playerId))
            return BadRequest(new { message = "Invalid player ID format" });

        var player = await _playerRepository.GetByIdAsync(playerId);
        if (player == null) return NotFound();
        return Ok(player);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,TournamentAdmin")]
    public async Task<IActionResult> Create([FromBody] Player player)
    {
        var created = await _playerRepository.CreateAsync(player);
        return CreatedAtAction(nameof(GetById), new { id = created.PlayerId }, created);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,TournamentAdmin")]
    public async Task<IActionResult> Update(string id, [FromBody] Player player)
    {
        if (!Guid.TryParse(id, out var playerId) || player.PlayerId != playerId)
            return BadRequest();

        var updated = await _playerRepository.UpdateAsync(player);
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,TournamentAdmin")]
    public async Task<IActionResult> Delete(string id)
    {
        if (!Guid.TryParse(id, out var playerId))
            return BadRequest(new { message = "Invalid player ID format" });

        var result = await _playerRepository.DeleteAsync(playerId);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPost("{id}/assign")]
    [Authorize(Roles = "Admin,TournamentAdmin")]
    public async Task<IActionResult> AssignToTeam(string id, [FromBody] AssignPlayerRequest request)
    {
        if (!Guid.TryParse(id, out var playerId))
            return BadRequest(new { message = "Invalid player ID format" });

        if (!Guid.TryParse(request.TeamId, out var teamId))
            return BadRequest(new { message = "Invalid team ID format" });

        var result = await _playerRepository.AssignToTeamAsync(playerId, teamId, request.SoldPrice);
        if (!result) return BadRequest();
        return Ok(new { message = "Player assigned successfully" });
    }
}

public class AssignPlayerRequest
{
    public string TeamId { get; set; } = string.Empty;
    public decimal SoldPrice { get; set; }
}

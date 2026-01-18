using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TournamentAuction.Domain.Entities;
using TournamentAuction.Domain.Interfaces;

namespace TournamentAuction.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PlayerCategoryController : ControllerBase
{
    private readonly IPlayerCategoryRepository _categoryRepository;

    public PlayerCategoryController(IPlayerCategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    [HttpGet("tournament/{tournamentId}")]
    public async Task<IActionResult> GetByTournament(string tournamentId)
    {
        if (!Guid.TryParse(tournamentId, out var tournamentIdGuid))
            return BadRequest(new { message = "Invalid tournament ID format" });

        var categories = await _categoryRepository.GetByTournamentIdAsync(tournamentIdGuid);
        return Ok(categories);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        if (!Guid.TryParse(id, out var categoryId))
            return BadRequest(new { message = "Invalid category ID format" });

        var category = await _categoryRepository.GetByIdAsync(categoryId);
        if (category == null) return NotFound();
        return Ok(category);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,TournamentAdmin")]
    public async Task<IActionResult> Create([FromBody] PlayerCategory category)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        category.CreatedBy = userId;
        category.UpdatedBy = userId;
        
        var created = await _categoryRepository.CreateAsync(category);
        return CreatedAtAction(nameof(GetById), new { id = created.CategoryId }, created);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,TournamentAdmin")]
    public async Task<IActionResult> Update(string id, [FromBody] PlayerCategory category)
    {
        if (!Guid.TryParse(id, out var categoryId) || category.CategoryId != categoryId)
            return BadRequest();
        
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        category.UpdatedBy = userId;
        
        var updated = await _categoryRepository.UpdateAsync(category);
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,TournamentAdmin")]
    public async Task<IActionResult> Delete(string id)
    {
        if (!Guid.TryParse(id, out var categoryId))
            return BadRequest(new { message = "Invalid category ID format" });

        var result = await _categoryRepository.DeleteAsync(categoryId);
        if (!result) return NotFound();
        return NoContent();
    }
}


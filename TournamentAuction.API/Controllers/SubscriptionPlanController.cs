using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TournamentAuction.Domain.Entities;
using TournamentAuction.Domain.Interfaces;

namespace TournamentAuction.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubscriptionPlanController : ControllerBase
{
    private readonly ISubscriptionPlanRepository _planRepository;

    public SubscriptionPlanController(ISubscriptionPlanRepository planRepository)
    {
        _planRepository = planRepository;
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActivePlans()
    {
        var plans = await _planRepository.GetAllActiveAsync();
        return Ok(plans);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        if (!Guid.TryParse(id, out var planId))
            return BadRequest(new { message = "Invalid plan ID format" });

        var plan = await _planRepository.GetByIdAsync(planId);
        if (plan == null) return NotFound();
        return Ok(plan);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] SubscriptionPlan plan)
    {
        if (plan.PlanId == Guid.Empty)
        {
            plan.PlanId = Guid.NewGuid();
        }
        plan.CreatedAt = DateTime.UtcNow;

        var created = await _planRepository.CreateAsync(plan);
        return CreatedAtAction(nameof(GetById), new { id = created.PlanId }, created);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(string id, [FromBody] SubscriptionPlan plan)
    {
        if (!Guid.TryParse(id, out var planId) || plan.PlanId != planId)
            return BadRequest();

        var updated = await _planRepository.UpdateAsync(plan);
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(string id)
    {
        if (!Guid.TryParse(id, out var planId))
            return BadRequest(new { message = "Invalid plan ID format" });

        var result = await _planRepository.DeleteAsync(planId);
        if (!result) return NotFound();
        return NoContent();
    }
}


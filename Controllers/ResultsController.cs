using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using System.Security.Claims;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ResultsController : ControllerBase
{
    private readonly RaceDayDbContext _context;
    public ResultsController(RaceDayDbContext context) => _context = context;

  
    [HttpPut("entries/{entryId}")]
    [Authorize(Policy = "OrganiserOnly")]
    public async Task<IActionResult> CaptureResult(Guid entryId, [FromBody] CaptureResultDto dto)
    {
        var organiserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var entry = await _context.EventEntries
            .Include(ee => ee.Registration).ThenInclude(r => r.Event)
            .FirstOrDefaultAsync(ee => ee.Event_entryId == entryId);
        if (entry == null) return NotFound(new { message = "Entry not found." });
        if (entry.Registration.Event.CompanyId != organiserId) return Forbid();

        entry.FinishTime = dto.FinishTime;
        entry.FinishingPosition = dto.FinishingPosition;
        entry.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok(new { message = "Result captured." });
    }

 
    [HttpGet("my-results")]
    [Authorize(Policy = "ParticipantOnly")]
    public async Task<IActionResult> GetMyResults()
    {
        var participantId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var results = await _context.EventEntries
            .Where(ee => ee.Registration.ParticipantId == participantId && ee.FinishTime != null)
            .Include(ee => ee.Registration).ThenInclude(r => r.Event)
            .Include(ee => ee.Route)
            .Select(ee => new
            {
                ee.Event_entryId,
                Event = new { ee.Registration.Event.EventName, ee.Registration.Event.EventDate, ee.Registration.Event.Location },
                Category = ee.Route.RouteName,
                Distance = ee.Route.DistanceKM,
                ee.FinishTime,
                ee.FinishingPosition
            }).ToListAsync();
        return Ok(results);
    }
}

public class CaptureResultDto
{
    public TimeSpan FinishTime { get; set; }
    public int FinishingPosition { get; set; }
}
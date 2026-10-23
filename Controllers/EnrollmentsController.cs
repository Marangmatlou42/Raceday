using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;
using System.Security.Claims;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EnrolmentsController : ControllerBase
{
    private readonly RaceDayDbContext _context;
    public EnrolmentsController(RaceDayDbContext context) => _context = context;

   
    [HttpPost("events/{eventId}")]
    [Authorize(Policy = "ParticipantOnly")]
    public async Task<IActionResult> Enrol(Guid eventId, [FromBody] EnrolDto dto)
    {
        var participantId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var ev = await _context.Events.Include(e => e.Routes).FirstOrDefaultAsync(e => e.EventId == eventId);
        if (ev == null) return NotFound(new { message = "Event not found." });
        if (!ev.IsPublished) return BadRequest(new { message = "Event not open." });

        var route = ev.Routes.FirstOrDefault(r => r.RouteId == dto.RouteId);
        if (route == null) return BadRequest(new { message = "Category not found." });

        var existing = await _context.Registrations
            .FirstOrDefaultAsync(r => r.ParticipantId == participantId && r.EventId == eventId);
        if (existing != null) return BadRequest(new { message = "Already enrolled." });

        var registration = new Registration
        {
            ParticipantId = participantId,
            EventId = eventId,
            Username = User.FindFirstValue(ClaimTypes.Email)!,
            Status = "confirmed"
        };
        _context.Registrations.Add(registration);
        await _context.SaveChangesAsync();

        var entry = new EventEntry
        {
            RegistrationId = registration.RegistrationId,
            RouteId = dto.RouteId,
            EntryStatus = "confirmed",
            EmergencyContactName = dto.EmergencyContactName,
            EmergencyContactPhone = dto.EmergencyContactPhone,
            MedicalConditions = dto.MedicalConditions,
            TShirtSize = dto.TShirtSize
        };
        _context.EventEntries.Add(entry);
        ev.CurrentEntries++;
        await _context.SaveChangesAsync();

        return StatusCode(201, new { message = "Enrolment successful.", registrationId = registration.RegistrationId, entryId = entry.Event_entryId });
    }

    
    [HttpGet("my-enrolments")]
    [Authorize(Policy = "ParticipantOnly")]
    public async Task<IActionResult> GetMyEnrolments()
    {
        var participantId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var enrolments = await _context.Registrations
            .Where(r => r.ParticipantId == participantId)
            .Include(r => r.Event)
            .Include(r => r.EventEntries).ThenInclude(ee => ee.Route)
            .Select(r => new
            {
                r.RegistrationId,
                r.Status,
                r.CreatedAt,
                Event = new { r.Event.EventId, r.Event.EventName, r.Event.EventDate, r.Event.Location },
                Entries = r.EventEntries.Select(ee => new { ee.Event_entryId, ee.EntryStatus, Category = ee.Route.RouteName, ee.FinishTime, ee.FinishingPosition })
            }).ToListAsync();
        return Ok(enrolments);
    }

   
    [HttpGet("organiser/enrolments")]
    [Authorize(Policy = "OrganiserOnly")]
    public async Task<IActionResult> GetOrganiserEnrolments()
    {
        var organiserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var enrolments = await _context.Registrations
            .Where(r => r.Event.CompanyId == organiserId)
            .Include(r => r.Participant).Include(r => r.Event)
            .Include(r => r.EventEntries).ThenInclude(ee => ee.Route)
            .Select(r => new
            {
                r.RegistrationId,
                r.Status,
                Participant = new { r.Participant.FirstName, r.Participant.LastName, r.Participant.ParticipantEmail, r.Participant.MedicalCondition },
                Event = new { r.Event.EventId, r.Event.EventName },
                Entries = r.EventEntries.Select(ee => new { ee.Event_entryId, ee.EntryStatus, Category = ee.Route.RouteName, ee.EmergencyContactName, ee.EmergencyContactPhone, ee.FinishTime, ee.FinishingPosition })
            }).ToListAsync();
        return Ok(enrolments);
    }
}

public class EnrolDto
{
    public Guid RouteId { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? MedicalConditions { get; set; }
    public string? TShirtSize { get; set; }
}

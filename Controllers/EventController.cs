using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;
using System.Security.Claims;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly RaceDayDbContext _context;
    public EventsController(RaceDayDbContext context) => _context = context;

   
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetEvents()
    {
        var events = await _context.Events
            .Where(e => e.IsPublished && !e.IsCancelled)
            .Include(e => e.EventType).Include(e => e.Organiser).Include(e => e.Routes)
            .Select(e => new
            {
                e.EventId,
                e.EventName,
                e.Description,
                e.EventDate,
                e.Location,
                e.Time,
                e.EntryFee,
                EventType = e.EventType.Name,
                Organiser = e.Organiser.CompanyName,
                Routes = e.Routes.Select(r => new { r.RouteId, r.RouteName, r.DistanceKM }),
                e.CurrentEntries,
                e.MaxParticipants
            }).ToListAsync();
        return Ok(events);
    }

    
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetEvent(Guid id)
    {
        var ev = await _context.Events.Include(e => e.EventType).Include(e => e.Organiser).Include(e => e.Routes)
            .FirstOrDefaultAsync(e => e.EventId == id);
        if (ev == null) return NotFound(new { message = "Event not found." });
        return Ok(new
        {
            ev.EventId,
            ev.EventName,
            ev.Description,
            ev.EventDate,
            ev.Location,
            ev.Time,
            ev.EntryFee,
            EventType = ev.EventType.Name,
            Organiser = ev.Organiser.CompanyName,
            Routes = ev.Routes.Select(r => new { r.RouteId, r.RouteName, r.DistanceKM, r.RouteDescription }),
            ev.CurrentEntries,
            ev.MaxParticipants,
            ev.IsPublished,
            ev.IsCancelled
        });
    }

   
    [HttpPost]
    [Authorize(Policy = "OrganiserOnly")]
    public async Task<IActionResult> CreateEvent([FromBody] CreateEventDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var organiserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var ev = new Event
        {
            CompanyId = organiserId,
            Event_typeId = dto.EventTypeId,
            EventName = dto.EventName,
            EventDate = dto.EventDate,
            Location = dto.Location,
            Time = dto.Time,
            Description = dto.Description,
            EntryFee = dto.EntryFee,
            MaxParticipants = dto.MaxParticipants,
            IsPublished = dto.IsPublished
        };
        _context.Events.Add(ev);
        await _context.SaveChangesAsync();
        return StatusCode(201, new { message = "Event created.", eventId = ev.EventId });
    }

    
    [HttpPut("{id}")]
    [Authorize(Policy = "OrganiserOnly")]
    public async Task<IActionResult> UpdateEvent(Guid id, [FromBody] UpdateEventDto dto)
    {
        var organiserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var ev = await _context.Events.FirstOrDefaultAsync(e => e.EventId == id);
        if (ev == null) return NotFound(new { message = "Event not found." });
        if (ev.CompanyId != organiserId) return Forbid();

        ev.EventName = dto.EventName ?? ev.EventName;
        ev.Description = dto.Description ?? ev.Description;
        ev.EventDate = dto.EventDate ?? ev.EventDate;
        ev.Location = dto.Location ?? ev.Location;
        ev.Time = dto.Time ?? ev.Time;
        ev.EntryFee = dto.EntryFee ?? ev.EntryFee;
        ev.IsPublished = dto.IsPublished ?? ev.IsPublished;
        ev.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok(new { message = "Event updated." });
    }

   
    [HttpDelete("{id}")]
    [Authorize(Policy = "OrganiserOnly")]
    public async Task<IActionResult> DeleteEvent(Guid id)
    {
        var organiserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var ev = await _context.Events.FirstOrDefaultAsync(e => e.EventId == id);
        if (ev == null) return NotFound(new { message = "Event not found." });
        if (ev.CompanyId != organiserId) return Forbid();
        _context.Events.Remove(ev);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Event deleted." });
    }
}

public class CreateEventDto
{
    public Guid EventTypeId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public TimeSpan Time { get; set; }
    public string? Description { get; set; }
    public decimal EntryFee { get; set; }
    public int? MaxParticipants { get; set; }
    public bool IsPublished { get; set; }
}

public class UpdateEventDto
{
    public string? EventName { get; set; }
    public string? Description { get; set; }
    public DateTime? EventDate { get; set; }
    public string? Location { get; set; }
    public TimeSpan? Time { get; set; }
    public decimal? EntryFee { get; set; }
    public bool? IsPublished { get; set; }
}
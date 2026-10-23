using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;
using System.Security.Claims;
using Route = RaceDay.API.Models.Route;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/events/{eventId}/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly RaceDayDbContext _context;
    public CategoriesController(RaceDayDbContext context) => _context = context;

   
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategories(Guid eventId)
    {
        var categories = await _context.Routes.Where(r => r.EventId == eventId)
            .Select(r => new { r.RouteId, r.RouteName, r.DistanceKM, r.RouteDescription, r.ElevationGain })
            .ToListAsync();
        return Ok(categories);
    }

    
    [HttpPost]
    [Authorize(Policy = "OrganiserOnly")]
    public async Task<IActionResult> AddCategory(Guid eventId, [FromBody] CreateRouteDto dto)
    {
        var organiserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var ev = await _context.Events.FirstOrDefaultAsync(e => e.EventId == eventId);
        if (ev == null) return NotFound(new { message = "Event not found." });
        if (ev.CompanyId != organiserId) return Forbid();

        var route = new Route
        {
            EventId = eventId,
            RouteName = dto.RouteName,
            DistanceKM = dto.DistanceKM,
            RouteDescription = dto.RouteDescription,
            ElevationGain = dto.ElevationGain
        };
        _context.Routes.Add(route);
        await _context.SaveChangesAsync();
        return StatusCode(201, new { message = "Category added.", routeId = route.RouteId });
    }
}

public class CreateRouteDto
{
    public string RouteName { get; set; } = string.Empty;
    public decimal DistanceKM { get; set; }
    public string? RouteDescription { get; set; }
    public int ElevationGain { get; set; }
}
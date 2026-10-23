using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using System.Security.Claims;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly RaceDayDbContext _context;
    public ProfileController(RaceDayDbContext context) => _context = context;

   
    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        var id = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var role = User.FindFirstValue(ClaimTypes.Role);

        if (role == "Participant")
        {
            var p = await _context.Participants.Where(x => x.ParticipantId == id)
                .Select(x => new { x.ParticipantId, x.FirstName, x.LastName, x.ParticipantEmail, x.DateOfBirth, x.MedicalCondition, Role = "Participant" })
                .FirstOrDefaultAsync();
            return p == null ? NotFound() : Ok(p);
        }
        else
        {
            var o = await _context.Organisers.Where(x => x.CompanyId == id)
                .Select(x => new { x.CompanyId, x.CompanyName, x.CompanyEmail, x.CompanyPhone, x.IsApproved, Role = "Organiser" })
                .FirstOrDefaultAsync();
            return o == null ? NotFound() : Ok(o);
        }
    }

   
    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var id = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var role = User.FindFirstValue(ClaimTypes.Role);

        if (role == "Participant")
        {
            var p = await _context.Participants.FindAsync(id);
            if (p == null) return NotFound();
            p.FirstName = dto.FirstName ?? p.FirstName;
            p.LastName = dto.LastName ?? p.LastName;
            p.MedicalCondition = dto.MedicalCondition ?? p.MedicalCondition;
            p.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            var o = await _context.Organisers.FindAsync(id);
            if (o == null) return NotFound();
            o.CompanyName = dto.CompanyName ?? o.CompanyName;
            o.CompanyPhone = dto.CompanyPhone ?? o.CompanyPhone;
            o.UpdatedAt = DateTime.UtcNow;
        }
        await _context.SaveChangesAsync();
        return Ok(new { message = "Profile updated." });
    }
}

public class UpdateProfileDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? MedicalCondition { get; set; }
    public string? CompanyName { get; set; }
    public string? CompanyPhone { get; set; }
}
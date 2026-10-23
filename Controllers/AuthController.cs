using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using RaceDay.API.Services;
using System.Security.Claims;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly RaceDayDbContext _context;
    private readonly IPasswordService _passwordService;

    public AuthController(RaceDayDbContext context, IPasswordService passwordService)
    {
        _context = context;
        _passwordService = passwordService;
    }

    
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        if (dto.Role.Equals("Participant", StringComparison.OrdinalIgnoreCase))
        {
            if (await _context.Participants.AnyAsync(p => p.ParticipantEmail == dto.Email))
                return BadRequest(new { message = "Email already registered." });

            var participant = new Participant
            {
                FirstName = dto.FirstName!,
                LastName = dto.LastName!,
                ParticipantEmail = dto.Email,
                DateOfBirth = dto.DateOfBirth!.Value,
                ParticipantPassword = _passwordService.HashPassword(dto.Password),
                MedicalCondition = dto.MedicalCondition
            };
            _context.Participants.Add(participant);
            await _context.SaveChangesAsync();
            return StatusCode(201, new { message = "Participant registered.", userId = participant.ParticipantId, role = "Participant" });
        }

        if (dto.Role.Equals("Organiser", StringComparison.OrdinalIgnoreCase))
        {
            if (await _context.Organisers.AnyAsync(o => o.CompanyEmail == dto.Email))
                return BadRequest(new { message = "Email already registered." });

            var organiser = new Organiser
            {
                CompanyName = dto.CompanyName!,
                CompanyEmail = dto.Email,
                CompanyPhone = dto.CompanyPhone!,
                CompanyPassword = _passwordService.HashPassword(dto.Password)
            };
            _context.Organisers.Add(organiser);
            await _context.SaveChangesAsync();
            return StatusCode(201, new { message = "Organiser registered. Awaiting approval.", userId = organiser.CompanyId, role = "Organiser" });
        }

        return BadRequest(new { message = "Role must be 'Participant' or 'Organiser'." });
    }

   
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        ClaimsIdentity? identity = null;
        string? displayName = null;

        if (dto.Role.Equals("Participant", StringComparison.OrdinalIgnoreCase))
        {
            var p = await _context.Participants.FirstOrDefaultAsync(x => x.ParticipantEmail == dto.Email);
            if (p == null || !_passwordService.VerifyPassword(dto.Password, p.ParticipantPassword))
                return Unauthorized(new { message = "Invalid credentials." });

            identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, p.ParticipantId.ToString()),
                new Claim(ClaimTypes.Email, p.ParticipantEmail),
                new Claim(ClaimTypes.Name, $"{p.FirstName} {p.LastName}"),
                new Claim(ClaimTypes.Role, "Participant")
            }, CookieAuthenticationDefaults.AuthenticationScheme);
            displayName = $"{p.FirstName} {p.LastName}";
        }
        else if (dto.Role.Equals("Organiser", StringComparison.OrdinalIgnoreCase))
        {
            var o = await _context.Organisers.FirstOrDefaultAsync(x => x.CompanyEmail == dto.Email);
            if (o == null || !_passwordService.VerifyPassword(dto.Password, o.CompanyPassword))
                return Unauthorized(new { message = "Invalid credentials." });
            if (!o.IsApproved)
                return Unauthorized(new { message = "Organiser not yet approved." });

            identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, o.CompanyId.ToString()),
                new Claim(ClaimTypes.Email, o.CompanyEmail),
                new Claim(ClaimTypes.Name, o.CompanyName),
                new Claim(ClaimTypes.Role, "Organiser")
            }, CookieAuthenticationDefaults.AuthenticationScheme);
            displayName = o.CompanyName;
        }

        if (identity == null) return BadRequest(new { message = "Invalid role." });

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTime.UtcNow.AddHours(8) });

        return Ok(new { message = "Login successful.", role = dto.Role, displayName });
    }

   
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok(new { message = "Logged out successfully." });
    }
}

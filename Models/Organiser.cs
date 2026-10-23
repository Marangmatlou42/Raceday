using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models;

public class Organiser
{
    [Key]
    public Guid CompanyId { get; set; } = Guid.NewGuid();

    [Required, MaxLength(200)]
    public string CompanyName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(255)]
    public string CompanyEmail { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string CompanyPhone { get; set; } = string.Empty;

    [Required]
    public string CompanyPassword { get; set; } = string.Empty;

    public bool IsApproved { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Event> Events { get; set; } = new List<Event>();
}

using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models;

public class EventType
{
    [Key]
    public Guid Event_typeId { get; set; } = Guid.NewGuid();

    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Event> Events { get; set; } = new List<Event>();
}

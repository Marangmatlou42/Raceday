using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models;

public class Registration
{
    [Key]
    public Guid RegistrationId { get; set; } = Guid.NewGuid();

    [Required]
    public Guid ParticipantId { get; set; }

    [Required]
    public Guid EventId { get; set; }

    [Required, MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [MaxLength(30)]
    public string Status { get; set; } = "pending";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(ParticipantId))]
    public Participant Participant { get; set; } = null!;

    [ForeignKey(nameof(EventId))]
    public Event Event { get; set; } = null!;

    public ICollection<EventEntry> EventEntries { get; set; } = new List<EventEntry>();
}

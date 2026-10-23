using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models;

public class Event
{
    [Key]
    public Guid EventId { get; set; } = Guid.NewGuid();

    [Required]
    public Guid CompanyId { get; set; }

    [Required]
    public Guid Event_typeId { get; set; }

    [Required, MaxLength(200)]
    public string EventName { get; set; } = string.Empty;

    [Required]
    public DateTime EventDate { get; set; }

    [Required, MaxLength(255)]
    public string Location { get; set; } = string.Empty;

    [Required]
    public TimeSpan Time { get; set; }

    public string? Description { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal EntryFee { get; set; } = 0.00m;

    public bool IsPublished { get; set; } = false;
    public bool IsCancelled { get; set; } = false;
    public int? MaxParticipants { get; set; }
    public int CurrentEntries { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(CompanyId))]
    public Organiser Organiser { get; set; } = null!;

    [ForeignKey(nameof(Event_typeId))]
    public EventType EventType { get; set; } = null!;

    public ICollection<Route> Routes { get; set; } = new List<Route>();
    public ICollection<Registration> Registrations { get; set; } = new List<Registration>();
}
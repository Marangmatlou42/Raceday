using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models;

public class EventEntry
{
    [Key]
    public Guid Event_entryId { get; set; } = Guid.NewGuid();

    [Required]
    public Guid RegistrationId { get; set; }

    [Required]
    public Guid RouteId { get; set; }

    [MaxLength(30)]
    public string EntryStatus { get; set; } = "pending";

    public DateTime EnteredAt { get; set; } = DateTime.UtcNow;

    [MaxLength(100)]
    public string? PaymentReference { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? PaymentAmount { get; set; }

    [MaxLength(30)]
    public string PaymentStatus { get; set; } = "unpaid";

    [MaxLength(20)]
    public string? BibNumber { get; set; }

    [MaxLength(200)]
    public string? EmergencyContactName { get; set; }

    [MaxLength(20)]
    public string? EmergencyContactPhone { get; set; }

    public string? MedicalConditions { get; set; }

    [MaxLength(10)]
    public string? TShirtSize { get; set; }

    public TimeSpan? FinishTime { get; set; }
    public int? FinishingPosition { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(RegistrationId))]
    public Registration Registration { get; set; } = null!;

    [ForeignKey(nameof(RouteId))]
    public Route Route { get; set; } = null!;
}
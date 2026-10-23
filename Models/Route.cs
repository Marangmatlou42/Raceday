using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models;

public class Route
{
    [Key]
    public Guid RouteId { get; set; } = Guid.NewGuid();

    [Required]
    public Guid EventId { get; set; }

    [Required, MaxLength(200)]
    public string RouteName { get; set; } = string.Empty;

    [Column(TypeName = "decimal(6,2)")]
    public decimal DistanceKM { get; set; }

    public string? RouteDescription { get; set; }
    public int ElevationGain { get; set; } = 0;
    public string? RouteGpxUrl { get; set; }
    public string? RouteMapUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(EventId))]
    public Event Event { get; set; } = null!;

    public ICollection<EventEntry> EventEntries { get; set; } = new List<EventEntry>();
}
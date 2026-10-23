using Microsoft.EntityFrameworkCore;
using RaceDay.API.Models;
using Route = RaceDay.API.Models.Route;

namespace RaceDay.API.Data;

public class RaceDayDbContext : DbContext
{
    public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options)
        : base(options) { }

    public DbSet<Participant> Participants => Set<Participant>();
    public DbSet<Organiser> Organisers => Set<Organiser>();
    public DbSet<EventType> EventTypes => Set<EventType>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Route> Routes => Set<Route>();
    public DbSet<Registration> Registrations => Set<Registration>();
    public DbSet<EventEntry> EventEntries => Set<EventEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Participant>().HasIndex(p => p.ParticipantEmail).IsUnique();
        modelBuilder.Entity<Organiser>().HasIndex(o => o.CompanyEmail).IsUnique();
        modelBuilder.Entity<EventType>().HasIndex(et => et.Name).IsUnique();
        modelBuilder.Entity<Registration>()
            .HasIndex(r => new { r.ParticipantId, r.EventId }).IsUnique();

        modelBuilder.Entity<Event>()
            .HasOne(e => e.Organiser).WithMany(o => o.Events)
            .HasForeignKey(e => e.CompanyId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Event>()
            .HasOne(e => e.EventType).WithMany(et => et.Events)
            .HasForeignKey(e => e.Event_typeId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Route>()
            .HasOne(r => r.Event).WithMany(e => e.Routes)
            .HasForeignKey(r => r.EventId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Registration>()
            .HasOne(r => r.Participant).WithMany(p => p.Registrations)
            .HasForeignKey(r => r.ParticipantId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Registration>()
            .HasOne(r => r.Event).WithMany(e => e.Registrations)
            .HasForeignKey(r => r.EventId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<EventEntry>()
            .HasOne(ee => ee.Registration).WithMany(r => r.EventEntries)
            .HasForeignKey(ee => ee.RegistrationId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<EventEntry>()
            .HasOne(ee => ee.Route).WithMany(r => r.EventEntries)
            .HasForeignKey(ee => ee.RouteId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<EventType>().HasData(
            new EventType { Event_typeId = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "Run", Description = "Road running events" },
            new EventType { Event_typeId = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "Walk", Description = "Walking events" },
            new EventType { Event_typeId = Guid.Parse("33333333-3333-3333-3333-333333333333"), Name = "Cycle", Description = "Cycling events" }
        );
    }
}

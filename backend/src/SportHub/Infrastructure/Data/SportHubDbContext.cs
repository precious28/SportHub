using Microsoft.EntityFrameworkCore;
using SportHub.Domain.Entities;

namespace SportHub.Infrastructure.Data;

public class SportHubDbContext(DbContextOptions<SportHubDbContext> options) : DbContext(options)
{
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Coach> Coaches => Set<Coach>();
    public DbSet<AgeGroup> AgeGroups => Set<AgeGroup>();
    public DbSet<Pitch> Pitches => Set<Pitch>();
    public DbSet<Fixture> Fixtures => Set<Fixture>();
    public DbSet<Registration> Registrations => Set<Registration>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Fixture>()
            .HasOne<Team>().WithMany().HasForeignKey(f => f.HomeTeamId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Fixture>()
            .HasOne<Team>().WithMany().HasForeignKey(f => f.AwayTeamId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Registration>()
            .HasIndex(r => new { r.EventId, r.TeamId }).IsUnique();
    }
}

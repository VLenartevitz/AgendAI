using AgendAI.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AgendAI.Api.Data;

public sealed class AgendAIDbContext(DbContextOptions<AgendAIDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Enterprise> Enterprises => Set<Enterprise>();
    public DbSet<Meeting> Meetings => Set<Meeting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(user => user.Id);
            entity.HasIndex(user => user.Email).IsUnique();
            entity.Property(user => user.FullName).HasMaxLength(120).IsRequired();
            entity.Property(user => user.Email).HasMaxLength(160).IsRequired();
            entity.Property(user => user.PasswordHash).HasMaxLength(400).IsRequired();
            entity.Property(user => user.WhatsAppNumber).HasMaxLength(32);
        });

        modelBuilder.Entity<Enterprise>(entity =>
        {
            entity.ToTable("enterprises");

            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();

            entity.Property(e => e.Name)
                .HasMaxLength(120)
                .IsRequired();

            entity.Property(e => e.Slug)
                .HasMaxLength(160)
                .IsRequired();

            entity.Property(e => e.Email)
                .HasMaxLength(160);

            entity.Property(e => e.PasswordHash)
                .HasMaxLength(400)
                .IsRequired();

            entity.Property(e => e.WhatsAppNumber)
                .HasMaxLength(32);

            entity.Property(e => e.CreatedAtUtc)
                .IsRequired();

            entity.Property(e => e.LastAccessAtUtc)
                .IsRequired();
        });

        modelBuilder.Entity<Meeting>(entity =>
        {
            entity.ToTable("meetings");
            entity.HasKey(meeting => meeting.Id);
            entity.HasIndex(meeting => new { meeting.EnterpriseId, meeting.ScheduledDate, meeting.StartTime });
            entity.Property(meeting => meeting.Title).HasMaxLength(120).IsRequired();
            entity.Property(meeting => meeting.ClientName).HasMaxLength(120).IsRequired();
            entity.Property(meeting => meeting.ClientPhone).HasMaxLength(32);
            entity.Property(meeting => meeting.Status).HasMaxLength(32).IsRequired();
            entity.Property(meeting => meeting.SourceChannel).HasMaxLength(32).IsRequired();
            entity.Property(meeting => meeting.Notes).HasMaxLength(600);

            entity.HasOne(meeting => meeting.Enterprise)
                .WithMany(enterprise => enterprise.Meetings)
                .HasForeignKey(meeting => meeting.EnterpriseId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

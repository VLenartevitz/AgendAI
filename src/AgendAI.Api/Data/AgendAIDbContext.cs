using AgendAI.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AgendAI.Api.Data;

public sealed class AgendAIDbContext(DbContextOptions<AgendAIDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Meeting> Meetings => Set<Meeting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(user => user.Id);
            entity.HasIndex(user => user.Email).IsUnique();
            entity.Property(user => user.FullName).HasMaxLength(120).IsRequired();
            entity.Property(user => user.BusinessName).HasMaxLength(120).IsRequired();
            entity.Property(user => user.Email).HasMaxLength(160).IsRequired();
            entity.Property(user => user.PasswordHash).HasMaxLength(400).IsRequired();
            entity.Property(user => user.WhatsAppNumber).HasMaxLength(32);
        });

        modelBuilder.Entity<Meeting>(entity =>
        {
            entity.ToTable("meetings");
            entity.HasKey(meeting => meeting.Id);
            entity.HasIndex(meeting => new { meeting.UserId, meeting.ScheduledDate, meeting.StartTime });
            entity.Property(meeting => meeting.Title).HasMaxLength(120).IsRequired();
            entity.Property(meeting => meeting.ClientName).HasMaxLength(120).IsRequired();
            entity.Property(meeting => meeting.ClientPhone).HasMaxLength(32);
            entity.Property(meeting => meeting.Status).HasMaxLength(32).IsRequired();
            entity.Property(meeting => meeting.SourceChannel).HasMaxLength(32).IsRequired();
            entity.Property(meeting => meeting.Notes).HasMaxLength(600);

            entity.HasOne(meeting => meeting.User)
                .WithMany(user => user.Meetings)
                .HasForeignKey(meeting => meeting.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

using Microsoft.EntityFrameworkCore;
using SmartDesk.Domain.Entities;

namespace SmartDesk.Infrastructure.Persistence;

public sealed class SmartDeskDbContext(DbContextOptions<SmartDeskDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TicketComment> TicketComments => Set<TicketComment>();
    public DbSet<TicketHistory> TicketHistories => Set<TicketHistory>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<SlaPolicy> SlaPolicies => Set<SlaPolicy>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(b => { b.ToTable("Users"); b.HasIndex(x => x.Email).IsUnique(); b.Property(x => x.Name).HasMaxLength(200).IsRequired(); b.Property(x => x.Email).HasMaxLength(320).IsRequired(); });
        modelBuilder.Entity<Ticket>(b => { b.ToTable("Tickets"); b.HasIndex(x => x.TicketNumber).IsUnique(); b.HasIndex(x => new { x.Status, x.Priority }); b.HasIndex(x => x.CustomerId); b.HasIndex(x => x.AssignedAgentId); b.HasIndex(x => x.CreatedAt); b.HasIndex(x => x.DueAt); b.Property(x => x.TicketNumber).HasMaxLength(32).IsRequired(); b.Property(x => x.Title).HasMaxLength(250).IsRequired(); b.Property(x => x.Description).HasMaxLength(8000).IsRequired(); b.Property(x => x.AiConfidence).HasPrecision(5, 4); b.Property(x => x.AiClassificationStatus).HasMaxLength(40); });
        modelBuilder.Entity<TicketComment>(b => { b.ToTable("TicketComments"); b.HasIndex(x => new { x.TicketId, x.CreatedAt }); b.Property(x => x.Content).HasMaxLength(8000).IsRequired(); });
        modelBuilder.Entity<TicketHistory>(b => { b.ToTable("TicketHistories"); b.HasIndex(x => new { x.TicketId, x.Timestamp }); b.Property(x => x.Action).HasMaxLength(100).IsRequired(); });
        modelBuilder.Entity<Category>(b => { b.ToTable("Categories"); b.HasIndex(x => x.Name).IsUnique(); b.Property(x => x.Name).HasMaxLength(100).IsRequired(); });
        modelBuilder.Entity<SlaPolicy>(b => { b.ToTable("SlaPolicies"); b.HasIndex(x => new { x.Priority, x.IsActive }); b.Property(x => x.Name).HasMaxLength(100).IsRequired(); });
        modelBuilder.Entity<Notification>(b => { b.ToTable("Notifications"); b.HasIndex(x => new { x.UserId, x.IsRead, x.CreatedAt }); b.Property(x => x.Type).HasMaxLength(100).IsRequired(); b.Property(x => x.Message).HasMaxLength(1000).IsRequired(); });
    }
}

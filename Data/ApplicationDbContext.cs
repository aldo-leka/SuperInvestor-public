using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace SuperInvestor.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Note> Notes { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<Research> Researches { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Note>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notes)
                .HasForeignKey(n => n.UserId);

        builder.Entity<Research>()
            .HasOne(r => r.User)
            .WithMany(u => u.Researches)
            .HasForeignKey(r => r.UserId);

        builder.Entity<Research>()
            .HasMany(r => r.Notes)
            .WithOne(n => n.Research)
            .HasForeignKey(n => n.ResearchId);
    }
}
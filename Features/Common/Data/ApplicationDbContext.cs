using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SuperInvestor.Features.Identity.Data;
using SuperInvestor.Features.Notes.Data;
using SuperInvestor.Features.Research.Data;
using SuperInvestor.Features.Subscription.Data;

namespace SuperInvestor.Features.Common.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Note> Notes { get; set; }
    public DbSet<SuperInvestor.Features.Subscription.Data.Subscription> Subscriptions { get; set; }
    public DbSet<SuperInvestor.Features.Research.Data.Research> Researches { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Note>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notes)
                .HasForeignKey(n => n.UserId);

        builder.Entity<SuperInvestor.Features.Research.Data.Research>()
            .HasOne(r => r.User)
            .WithMany(u => u.Researches)
            .HasForeignKey(r => r.UserId);

        builder.Entity<SuperInvestor.Features.Research.Data.Research>()
            .HasMany(r => r.Notes)
            .WithOne(n => n.Research)
            .HasForeignKey(n => n.ResearchId);
    }
}
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SuperInvestor.Features.Identity.Data;
using SuperInvestor.Features.Notes.Data;
using SuperInvestor.Features.Subscription.Data;

namespace SuperInvestor.Features.Common.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Note> Notes { get; set; }
    public DbSet<UserSubscription> UserSubscriptions { get; set; }
    public DbSet<Research.Data.Research> Researches { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Note>()
            .HasOne(n => n.User)
            .WithMany(u => u.Notes)
            .HasForeignKey(n => n.UserId);

        builder.Entity<Research.Data.Research>()
            .HasOne(r => r.User)
            .WithMany(u => u.Researches)
            .HasForeignKey(r => r.UserId);

        builder.Entity<Research.Data.Research>()
            .HasMany(r => r.Notes)
            .WithOne(n => n.Research)
            .HasForeignKey(n => n.ResearchId);
        
        builder.Entity<UserSubscription>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.StripeSubscriptionId).IsUnique();
            entity.HasIndex(e => e.UserId);
                
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.StripeSubscriptionId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.StripeCustomerId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PriceId).HasMaxLength(100);
            
            entity.Property(e => e.Status)
                .HasConversion<string>()
                .IsRequired();

            entity.HasOne(e => e.User)
                .WithOne(u => u.Subscription)
                .HasForeignKey<UserSubscription>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
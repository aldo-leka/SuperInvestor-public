using Microsoft.EntityFrameworkCore;
using SuperInvestor.Features.Common.Data;
using SuperInvestor.Features.Identity.Data;

namespace SuperInvestor.Features.Subscription.Services;

public class SubscriptionService(IDbContextFactory<ApplicationDbContext> factory)
{
    public async Task<bool> HasActiveSubscription(ApplicationUser user)
    {
        await using var db = await factory.CreateDbContextAsync();
        
        var subscription = await db.Subscriptions
            .FirstOrDefaultAsync(s => s.UserId == user.Id);

        if (subscription == null)
        {
            return false;
        }

        // Check if the subscription is active and either has no end date or the end date is in the future
        return subscription.Status == "active" && 
               (subscription.EndDate == null || subscription.EndDate > DateTime.UtcNow);
    }

    public string CreateBillingPortalSession(string customerId, string returnUrl)
    {
        var options = new Stripe.BillingPortal.SessionCreateOptions
        {
            Customer = customerId,
            ReturnUrl = returnUrl
        };

        var service = new Stripe.BillingPortal.SessionService();
        var session = service.Create(options);

        return session.Url;
    }

    private async Task<SuperInvestor.Features.Subscription.Data.Subscription> GetSubscription(int subscriptionId)
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.Subscriptions.FindAsync(subscriptionId);
    }

    public async Task<SuperInvestor.Features.Subscription.Data.Subscription> GetSubscription(string userId)
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.Subscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId);
    }

    public async Task<SuperInvestor.Features.Subscription.Data.Subscription> GetSubscriptionByStripeSubscriptionId(string stripeSubscriptionId)
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.Subscriptions.FirstOrDefaultAsync(s => s.StripeSubscriptionId == stripeSubscriptionId);
    }

    public async Task<bool> UpdateSubscription(SuperInvestor.Features.Subscription.Data.Subscription subscription)
    {
        await using var db = await factory.CreateDbContextAsync();
        db.Subscriptions.Update(subscription);
        var savedChanges = await db.SaveChangesAsync();
        return savedChanges > 0;
    }

    public async Task<bool> CancelSubscription(
        int subscriptionId,
        string status,
        DateTime? currentPeriodEnd)
    {
        await using var db = await factory.CreateDbContextAsync();
        var subscription = await GetSubscription(subscriptionId);
        subscription.Status = status;
        subscription.EndDate = currentPeriodEnd;
        subscription.CurrentPeriodEnd = currentPeriodEnd;

        return await db.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateSubscriptionStatus(int subscriptionId, string status)
    {
        await using var db = await factory.CreateDbContextAsync();
        var subscription = await GetSubscription(subscriptionId);
        subscription.Status = status;

        return await db.SaveChangesAsync() > 0;
    }

    public async Task<SuperInvestor.Features.Subscription.Data.Subscription> CreateSubscription(SuperInvestor.Features.Subscription.Data.Subscription subscription)
    {
        await using var db = await factory.CreateDbContextAsync();
        await db.Subscriptions.AddAsync(subscription);
        await db.SaveChangesAsync();
        return subscription;
    }
}
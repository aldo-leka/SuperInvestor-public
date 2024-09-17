using Microsoft.EntityFrameworkCore;
using SuperInvestor.Data;

namespace SuperInvestor.Services;

public class SubscriptionService(ApplicationDbContext dbContext)
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    public async Task<bool> HasActiveSubscription(ApplicationUser user)
    {
        await _semaphore.WaitAsync();
        try
        {
            var subscription = await _dbContext.Subscriptions
                .FirstOrDefaultAsync(s => s.UserId == user.Id);

            if (subscription == null)
            {
                return false;
            }

            // Check if the subscription is active and either has no end date or the end date is in the future
            return subscription.Status == "active" && 
                   (subscription.EndDate == null || subscription.EndDate > DateTime.UtcNow);
        }
        finally
        {
            _semaphore.Release();
        }
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

    public async Task<Subscription> GetSubscription(int subscriptionId)
    {
        await _semaphore.WaitAsync();
        try
        {
            return await _dbContext.Subscriptions.FindAsync(subscriptionId);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<Subscription> GetSubscription(string userId)
    {
        await _semaphore.WaitAsync();
        try
        {
            return await _dbContext.Subscriptions
                .FirstOrDefaultAsync(s => s.UserId == userId);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<Subscription> GetSubscriptionByStripeSubscriptionId(string stripeSubscriptionId)
    {
        await _semaphore.WaitAsync();
        try
        {
            return await _dbContext.Subscriptions.FirstOrDefaultAsync(s => s.StripeSubscriptionId == stripeSubscriptionId);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> UpdateSubscription(Subscription subscription)
    {
        await _semaphore.WaitAsync();
        try
        {
            _dbContext.Subscriptions.Update(subscription);
            var savedChanges = await _dbContext.SaveChangesAsync();
            return savedChanges > 0;
        }
        catch (DbUpdateConcurrencyException)
        {
            // Handle concurrency issues if necessary
            return false;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> CancelSubscription(
        int subscriptionId,
        string status,
        DateTime? currentPeriodEnd)
    {
        await _semaphore.WaitAsync();
        try
        {
            var subscription = await GetSubscription(subscriptionId);
            subscription.Status = status;
            subscription.EndDate = currentPeriodEnd;
            subscription.CurrentPeriodEnd = currentPeriodEnd;

            return await _dbContext.SaveChangesAsync() > 0;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> UpdateSubscriptionStatus(int subscriptionId, string status)
    {
        await _semaphore.WaitAsync();
        try
        {
            var subscription = await GetSubscription(subscriptionId);
            subscription.Status = status;

            return await _dbContext.SaveChangesAsync() > 0;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<Subscription> CreateSubscription(Subscription subscription)
    {
        await _semaphore.WaitAsync();
        try
        {
            await _dbContext.Subscriptions.AddAsync(subscription);
            await _dbContext.SaveChangesAsync();
            return subscription;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
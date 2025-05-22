using Microsoft.EntityFrameworkCore;
using Stripe;
using SuperInvestor.Features.Common.Data;
using SuperInvestor.Features.Subscription.Data;

namespace SuperInvestor.Features.Subscription.Services;

public class SubscriptionService(IDbContextFactory<ApplicationDbContext> factory, ILogger<SubscriptionService> logger) : ISubscriptionService
{
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

    public async Task CreateOrUpdateSubscriptionAsync(string userId, Stripe.Subscription subscription)
    {
        try
        {
            await using var db = await factory.CreateDbContextAsync();
            var existingSubscription = await db.UserSubscriptions
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (existingSubscription != null)
            {
                UpdateSubscriptionFromStripe(existingSubscription, subscription);
                db.UserSubscriptions.Update(existingSubscription);
            }
            else
            {
                var newSubscription = CreateSubscriptionFromStripe(userId, subscription);
                await db.UserSubscriptions.AddAsync(newSubscription);
            }

            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating/updating subscription for user {UserId}", userId);
            throw;
        }
    }

    public async Task HandleInvoicePaidAsync(string userId, Invoice invoice)
    {
        try
        {
            var subscription = await GetByUserIdAsync(userId);
            if (subscription == null)
            {
                logger.LogWarning("No subscription found for user {UserId} when handling paid invoice", userId);
                return;
            }

            // Update subscription status to active
            subscription.Status = SubscriptionStatus.Active;
            
            // Try to get period end from invoice line items if available
            var firstLineItem = invoice.Lines?.Data?.FirstOrDefault();
            if (firstLineItem?.Period != null)
            {
                subscription.CurrentPeriodEnd = firstLineItem.Period.End;
            }
            else if (invoice.PeriodEnd != DateTime.MinValue)
            {
                subscription.CurrentPeriodEnd = invoice.PeriodEnd;
            }
            
            subscription.LastInvoiceId = invoice.Id;
            subscription.UpdatedAt = DateTime.UtcNow;

            // Clear any past due status
            subscription.PastDue = false;
            subscription.PaymentFailedCount = 0;

            await using var db = await factory.CreateDbContextAsync();
            db.UserSubscriptions.Update(subscription);
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling paid invoice for user {UserId}", userId);
            throw;
        }
    }

    public async Task HandleInvoicePaymentFailedAsync(string userId, Invoice invoice)
    {
        try
        {
            var subscription = await GetByUserIdAsync(userId);
            if (subscription == null)
            {
                logger.LogWarning("No subscription found for user {UserId} when handling failed payment", userId);
                return;
            }

            // Update subscription with failure information
            subscription.PastDue = true;
            subscription.PaymentFailedCount = (subscription.PaymentFailedCount ?? 0) + 1;
            subscription.LastPaymentFailure = DateTime.UtcNow;
            subscription.UpdatedAt = DateTime.UtcNow;

            // If this is the final attempt (you can customize this logic)
            if (subscription.PaymentFailedCount >= 3)
            {
                subscription.Status = SubscriptionStatus.Unpaid;
                await DowngradeUserFeaturesAsync(userId);
            }
            else
            {
                subscription.Status = SubscriptionStatus.PastDue;
            }

            await using var db = await factory.CreateDbContextAsync();
            db.UserSubscriptions.Update(subscription);
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling payment failure for user {UserId}", userId);
            throw;
        }
    }

    public async Task UpdateSubscriptionStatusAsync(string userId, Stripe.Subscription subscription)
    {
        try
        {
            var userSubscription = await GetByUserIdAsync(userId);
            if (userSubscription == null)
            {
                logger.LogWarning("No subscription found for user {UserId} when updating status", userId);
                return;
            }

            UpdateSubscriptionFromStripe(userSubscription, subscription);
            
            await using var db = await factory.CreateDbContextAsync();
            db.UserSubscriptions.Update(userSubscription);
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating subscription status for user {UserId}", userId);
            throw;
        }
    }

    public async Task UpdateSubscriptionAsync(string userId, Stripe.Subscription subscription)
    {
        try
        {
            var userSubscription = await GetByUserIdAsync(userId);
            if (userSubscription == null)
            {
                // Create new subscription if it doesn't exist
                await CreateOrUpdateSubscriptionAsync(userId, subscription);
                return;
            }

            UpdateSubscriptionFromStripe(userSubscription, subscription);
            
            await using var db = await factory.CreateDbContextAsync();
            db.UserSubscriptions.Update(userSubscription);
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating subscription for user {UserId}", userId);
            throw;
        }
    }

    public async Task CancelSubscriptionAsync(string userId, Stripe.Subscription subscription)
    {
        try
        {
            var userSubscription = await GetByUserIdAsync(userId);
            if (userSubscription == null)
            {
                logger.LogWarning("No subscription found for user {UserId} when canceling", userId);
                return;
            }

            userSubscription.Status = SubscriptionStatus.Canceled;
            userSubscription.CanceledAt = DateTime.UtcNow;
            userSubscription.UpdatedAt = DateTime.UtcNow;

            // Keep access until the current period ends
            var subscriptionItem = subscription.Items?.Data?.FirstOrDefault();
            if (subscriptionItem != null)
            {
                userSubscription.CurrentPeriodEnd = subscriptionItem.CurrentPeriodEnd;
            }

            await using var db = await factory.CreateDbContextAsync();
            db.UserSubscriptions.Update(userSubscription);
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error canceling subscription for user {UserId}", userId);
            throw;
        }
    }

    public async Task<UserSubscription?> GetByUserIdAsync(string userId)
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.UserSubscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId);
    }

    public async Task<UserSubscription?> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId)
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.UserSubscriptions
            .FirstOrDefaultAsync(s => s.StripeSubscriptionId == stripeSubscriptionId);
    }
    
    private UserSubscription CreateSubscriptionFromStripe(string userId, Stripe.Subscription subscription)
    {
        // Get the subscription item for date information
        var subscriptionItem = subscription.Items?.Data?.FirstOrDefault();
        
        return new UserSubscription
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            StripeSubscriptionId = subscription.Id,
            StripeCustomerId = subscription.CustomerId,
            Status = MapStripeStatus(subscription.Status),
            PriceId = subscriptionItem?.Price?.Id,
            Quantity = subscriptionItem?.Quantity ?? 1,
            
            // Get period dates from the subscription item
            CurrentPeriodStart = subscriptionItem != null
                ? subscriptionItem.CurrentPeriodStart
                : DateTime.UtcNow,
                
            CurrentPeriodEnd = subscriptionItem != null
                ? subscriptionItem.CurrentPeriodEnd
                : DateTime.UtcNow.AddMonths(1),
                
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private void UpdateSubscriptionFromStripe(UserSubscription userSubscription, Stripe.Subscription subscription)
    {
        userSubscription.Status = MapStripeStatus(subscription.Status);
        
        var subscriptionItem = subscription.Items?.Data?.FirstOrDefault();
        
        if (subscriptionItem != null)
        {
            userSubscription.PriceId = subscriptionItem.Price?.Id;
            userSubscription.Quantity = subscriptionItem.Quantity;
            userSubscription.CurrentPeriodStart = subscriptionItem.CurrentPeriodStart;
            userSubscription.CurrentPeriodEnd = subscriptionItem.CurrentPeriodEnd;
        }

        if (subscription.CanceledAt.HasValue)
        {
            userSubscription.CanceledAt = subscription.CanceledAt;
        }

        userSubscription.UpdatedAt = DateTime.UtcNow;
    }

    private static SubscriptionStatus MapStripeStatus(string stripeStatus)
    {
        return stripeStatus.ToLower() switch
        {
            "active" => SubscriptionStatus.Active,
            "canceled" => SubscriptionStatus.Canceled,
            "incomplete" => SubscriptionStatus.Incomplete,
            "incomplete_expired" => SubscriptionStatus.IncompleteExpired,
            "past_due" => SubscriptionStatus.PastDue,
            "trialing" => SubscriptionStatus.Trialing,
            "unpaid" => SubscriptionStatus.Unpaid,
            _ => SubscriptionStatus.Active
        };
    }

    private async Task DowngradeUserFeaturesAsync(string userId)
    {
        // Implement feature downgrade logic here
    }
}
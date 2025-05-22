using Stripe;
using SuperInvestor.Features.Subscription.Data;

namespace SuperInvestor.Features.Subscription.Services;

public interface ISubscriptionService
{
    string CreateBillingPortalSession(string customerId, string returnUrl);
    Task CreateOrUpdateSubscriptionAsync(string userId, Stripe.Subscription subscription);
    Task HandleInvoicePaidAsync(string userId, Invoice invoice);
    Task HandleInvoicePaymentFailedAsync(string userId, Invoice invoice);
    Task UpdateSubscriptionStatusAsync(string userId, Stripe.Subscription subscription);
    Task UpdateSubscriptionAsync(string userId, Stripe.Subscription subscription);
    Task CancelSubscriptionAsync(string userId, Stripe.Subscription subscription);
    Task<UserSubscription?> GetByUserIdAsync(string userId);
    Task<UserSubscription?> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId);
}
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using SuperInvestor.Features.Common.Services;
using SuperInvestor.Features.Identity.Services;

namespace SuperInvestor.Features.Subscription.Controllers;

[Route("webhook")]
[ApiController]
public class StripeWebhookController(ILogger<StripeWebhookController> logger, UserService userService, Services.SubscriptionService subscriptionService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Index()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var secret = EnvironmentHelper.StripeWebHookSecret;

        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                json,
                Request.Headers["Stripe-Signature"],
                secret
            );

            switch (stripeEvent.Type)
            {
                case EventTypes.CheckoutSessionCompleted:
                    await HandleCheckoutSessionCompletedAsync(stripeEvent);
                    break;
                case EventTypes.CustomerSubscriptionUpdated:
                    await HandleCustomerSubscriptionUpdatedAsync(stripeEvent);
                    break;
                case EventTypes.CustomerSubscriptionDeleted:
                    await HandleCustomerSubscriptionDeletedAsync(stripeEvent);
                    break;
                case EventTypes.InvoicePaymentSucceeded:
                    await HandleInvoicePaymentSucceededAsync(stripeEvent);
                    break;
                case EventTypes.InvoicePaymentFailed:
                    await HandleInvoicePaymentFailedAsync(stripeEvent);
                    break;
                default:
                    logger.LogInformation("Unhandled event type: {0}", stripeEvent.Type);
                    break;
            }

            return Ok();
        }
        catch (StripeException e)
        {
            logger.LogError(e, "Error processing Stripe webhook");
            return BadRequest("Invalid payload");
        }
    }

    private async Task HandleCheckoutSessionCompletedAsync(Event stripeEvent)
    {
        var session = stripeEvent.Data.Object as Session;
        var options = new SessionGetOptions();
        options.AddExpand("line_items");
        options.AddExpand("customer");
        options.AddExpand("subscription");

        var service = new SessionService();
        var sessionWithLineItems = await service.GetAsync(session.Id, options);

        var userEmail = sessionWithLineItems.CustomerDetails.Email;
        var user = await userService.GetUserByEmailAsync(userEmail);

        if (user == null)
        {
            logger.LogError("User not found for email: {Email}", userEmail);
            return;
        }

        var stripeSubscription = sessionWithLineItems.Subscription;
        var subscriptionItem = stripeSubscription.Items.Data[0];
        var price = subscriptionItem.Price;

        var dbSubscription = new SuperInvestor.Features.Subscription.Data.Subscription
        {
            UserId = user.Id,
            StripeCustomerId = sessionWithLineItems.CustomerId,
            StripeSubscriptionId = stripeSubscription.Id,
            StartDate = DateTime.UtcNow,
            Status = stripeSubscription.Status,
            PlanId = price.Id,
            CurrentPeriodStart = subscriptionItem.CurrentPeriodStart,
            CurrentPeriodEnd = subscriptionItem.CurrentPeriodEnd,
            PlanName = price.Nickname,
            PlanAmount = price.UnitAmount ?? 0,
            PlanCurrency = price.Currency,
            PlanInterval = price.Recurring.Interval
        };

        await subscriptionService.CreateSubscription(dbSubscription);

        logger.LogInformation("Subscription created for user: {UserId}", user.Id);
    }

    private async Task HandleCustomerSubscriptionUpdatedAsync(Event stripeEvent)
    {
        var stripeSubscription = stripeEvent.Data.Object as Stripe.Subscription;
        var dbSubscription = await subscriptionService.GetSubscriptionByStripeSubscriptionId(stripeSubscription.Id);

        if (dbSubscription == null)
        {
            logger.LogError("Subscription not found: {SubscriptionId}", stripeSubscription.Id);
            return;
        }

        var subscriptionItem = stripeSubscription.Items.Data[0];
        var price = subscriptionItem.Price;
        dbSubscription.Status = stripeSubscription.Status;
        dbSubscription.PlanId = price.Id;
        dbSubscription.EndDate = stripeSubscription.CancelAtPeriodEnd ? subscriptionItem.CurrentPeriodEnd : null;
        dbSubscription.CurrentPeriodStart = subscriptionItem.CurrentPeriodStart;
        dbSubscription.CurrentPeriodEnd = subscriptionItem.CurrentPeriodEnd;
        dbSubscription.PlanName = price.Nickname;
        dbSubscription.PlanAmount = price.UnitAmount ?? 0;
        dbSubscription.PlanCurrency = price.Currency;
        dbSubscription.PlanInterval = price.Recurring.Interval;

        await subscriptionService.UpdateSubscription(dbSubscription);

        logger.LogInformation("Subscription updated: {SubscriptionId}", stripeSubscription.Id);
    }

    private async Task HandleCustomerSubscriptionDeletedAsync(Event stripeEvent)
    {
        var stripeSubscription = stripeEvent.Data.Object as Stripe.Subscription;
        var dbSubscription = await subscriptionService.GetSubscriptionByStripeSubscriptionId(stripeSubscription.Id);

        if (dbSubscription == null)
        {
            logger.LogError("Subscription not found: {SubscriptionId}", stripeSubscription.Id);
            return;
        }

        var subscriptionItem = stripeSubscription.Items.Data[0];
        await subscriptionService.CancelSubscription(
            dbSubscription.Id,
            "canceled",
            subscriptionItem.CurrentPeriodEnd);

        logger.LogInformation("Subscription canceled: {SubscriptionId}", stripeSubscription.Id);
    }

    private async Task HandleInvoicePaymentSucceededAsync(Event stripeEvent)
    {
        var invoice = stripeEvent.Data.Object as Invoice;
        
        // Expand the invoice to get the lines which contain subscription information
        var invoiceService = new InvoiceService();
        var invoiceOptions = new InvoiceGetOptions
        {
            Expand = new List<string> { "lines" }
        };
        var invoiceWithLines = await invoiceService.GetAsync(invoice.Id, invoiceOptions);
        
        // Find the subscription ID from the lines
        string subscriptionId = null;
        foreach (var line in invoiceWithLines.Lines.Data)
        {
            if (line.Subscription != null)
            {
                subscriptionId = line.Subscription.Id;
                break;
            }
        }
        
        if (string.IsNullOrEmpty(subscriptionId))
        {
            logger.LogError("No subscription found in invoice: {InvoiceId}", invoice.Id);
            return;
        }
        
        var subscription = await subscriptionService.GetSubscriptionByStripeSubscriptionId(subscriptionId);

        if (subscription == null)
        {
            logger.LogError("Subscription not found in database: {SubscriptionId}", subscriptionId);
            return;
        }

        // Update subscription status if needed
        if (subscription.Status != "active")
        {
            await subscriptionService.UpdateSubscriptionStatus(subscription.Id, "active");
        }

        logger.LogInformation("Payment succeeded for subscription: {SubscriptionId}", subscriptionId);
    }

    private async Task HandleInvoicePaymentFailedAsync(Event stripeEvent)
    {
        var invoice = stripeEvent.Data.Object as Invoice;
        
        // Expand the invoice to get the lines which contain subscription information
        var invoiceService = new InvoiceService();
        var invoiceOptions = new InvoiceGetOptions
        {
            Expand = new List<string> { "lines" }
        };
        var invoiceWithLines = await invoiceService.GetAsync(invoice.Id, invoiceOptions);
        
        // Find the subscription ID from the lines
        string subscriptionId = null;
        foreach (var line in invoiceWithLines.Lines.Data)
        {
            if (line.Subscription != null)
            {
                subscriptionId = line.Subscription.Id;
                break;
            }
        }
        
        if (string.IsNullOrEmpty(subscriptionId))
        {
            logger.LogError("No subscription found in invoice: {InvoiceId}", invoice.Id);
            return;
        }
        
        var subscription = await subscriptionService.GetSubscriptionByStripeSubscriptionId(subscriptionId);

        if (subscription == null)
        {
            logger.LogError("Subscription not found in database: {SubscriptionId}", subscriptionId);
            return;
        }

        // Update subscription status
        await subscriptionService.UpdateSubscriptionStatus(subscription.Id, "past_due");

        logger.LogWarning("Payment failed for subscription: {SubscriptionId}", subscriptionId);

        // TODO: Implement logic to notify the user about the failed payment
    }
}
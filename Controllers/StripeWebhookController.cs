using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using SuperInvestor.Services;

namespace SuperInvestor.Controllers;

[Route("webhook")]
[ApiController]
public class StripeWebhookController(IConfiguration configuration, ILogger<StripeWebhookController> logger, UserService userService, Services.SubscriptionService subscriptionService) : ControllerBase
{
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<StripeWebhookController> _logger = logger;
    private readonly UserService _userService = userService;
    private readonly Services.SubscriptionService _subscriptionService = subscriptionService;

    [HttpPost]
    public async Task<IActionResult> Index()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var secret = _configuration["StripeWebHookSecret"];

        if (string.IsNullOrEmpty(secret))
        {
            _logger.LogError("Stripe webhook secret is not configured");
            return StatusCode(500, "Stripe webhook secret is not configured");
        }

        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                json,
                Request.Headers["Stripe-Signature"],
                secret
            );

            switch (stripeEvent.Type)
            {
                case Events.CheckoutSessionCompleted:
                    await HandleCheckoutSessionCompletedAsync(stripeEvent);
                    break;
                case Events.CustomerSubscriptionUpdated:
                    await HandleCustomerSubscriptionUpdatedAsync(stripeEvent);
                    break;
                case Events.CustomerSubscriptionDeleted:
                    await HandleCustomerSubscriptionDeletedAsync(stripeEvent);
                    break;
                case Events.InvoicePaymentSucceeded:
                    await HandleInvoicePaymentSucceededAsync(stripeEvent);
                    break;
                case Events.InvoicePaymentFailed:
                    await HandleInvoicePaymentFailedAsync(stripeEvent);
                    break;
                default:
                    _logger.LogInformation("Unhandled event type: {0}", stripeEvent.Type);
                    break;
            }

            return Ok();
        }
        catch (StripeException e)
        {
            _logger.LogError(e, "Error processing Stripe webhook");
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
        var user = await _userService.GetUserByEmailAsync(userEmail);

        if (user == null)
        {
            _logger.LogError("User not found for email: {Email}", userEmail);
            return;
        }

        var stripeSubscription = sessionWithLineItems.Subscription;
        var price = stripeSubscription.Items.Data[0].Price;

        var dbSubscription = new Data.Subscription
        {
            UserId = user.Id,
            StripeCustomerId = sessionWithLineItems.CustomerId,
            StripeSubscriptionId = stripeSubscription.Id,
            StartDate = DateTime.UtcNow,
            Status = stripeSubscription.Status,
            PlanId = price.Id,
            CurrentPeriodStart = stripeSubscription.CurrentPeriodStart,
            CurrentPeriodEnd = stripeSubscription.CurrentPeriodEnd,
            PlanName = price.Nickname,
            PlanAmount = price.UnitAmount ?? 0,
            PlanCurrency = price.Currency,
            PlanInterval = price.Recurring.Interval
        };

        await _subscriptionService.CreateSubscription(dbSubscription);

        _logger.LogInformation("Subscription created for user: {UserId}", user.Id);
    }

    private async Task HandleCustomerSubscriptionUpdatedAsync(Event stripeEvent)
    {
        var stripeSubscription = stripeEvent.Data.Object as Subscription;
        var dbSubscription = await _subscriptionService.GetSubscriptionByStripeSubscriptionId(stripeSubscription.Id);

        if (dbSubscription == null)
        {
            _logger.LogError("Subscription not found: {SubscriptionId}", stripeSubscription.Id);
            return;
        }

        var price = stripeSubscription.Items.Data[0].Price;
        dbSubscription.Status = stripeSubscription.Status;
        dbSubscription.PlanId = price.Id;
        dbSubscription.EndDate = stripeSubscription.CancelAtPeriodEnd ? stripeSubscription.CurrentPeriodEnd : null;
        dbSubscription.CurrentPeriodStart = stripeSubscription.CurrentPeriodStart;
        dbSubscription.CurrentPeriodEnd = stripeSubscription.CurrentPeriodEnd;
        dbSubscription.PlanName = price.Nickname;
        dbSubscription.PlanAmount = price.UnitAmount ?? 0;
        dbSubscription.PlanCurrency = price.Currency;
        dbSubscription.PlanInterval = price.Recurring.Interval;

        await _subscriptionService.UpdateSubscription(dbSubscription);

        _logger.LogInformation("Subscription updated: {SubscriptionId}", stripeSubscription.Id);
    }

    private async Task HandleCustomerSubscriptionDeletedAsync(Event stripeEvent)
    {
        var stripeSubscription = stripeEvent.Data.Object as Subscription;
        var dbSubscription = await _subscriptionService.GetSubscriptionByStripeSubscriptionId(stripeSubscription.Id);

        if (dbSubscription == null)
        {
            _logger.LogError("Subscription not found: {SubscriptionId}", stripeSubscription.Id);
            return;
        }

        await _subscriptionService.CancelSubscription(
            dbSubscription.Id,
            "canceled",
            stripeSubscription.CurrentPeriodEnd);

        _logger.LogInformation("Subscription canceled: {SubscriptionId}", stripeSubscription.Id);
    }

    private async Task HandleInvoicePaymentSucceededAsync(Event stripeEvent)
    {
        var invoice = stripeEvent.Data.Object as Invoice;
        var subscription = await _subscriptionService.GetSubscriptionByStripeSubscriptionId(invoice.SubscriptionId);

        if (subscription == null)
        {
            _logger.LogError("Subscription not found: {SubscriptionId}", invoice.SubscriptionId);
            return;
        }

        // Update subscription status if needed
        if (subscription.Status != "active")
        {
            await _subscriptionService.UpdateSubscriptionStatus(subscription.Id, "active");
        }

        _logger.LogInformation("Payment succeeded for subscription: {SubscriptionId}", invoice.SubscriptionId);
    }

    private async Task HandleInvoicePaymentFailedAsync(Event stripeEvent)
    {
        var invoice = stripeEvent.Data.Object as Invoice;
        var subscription = await _subscriptionService.GetSubscriptionByStripeSubscriptionId(invoice.SubscriptionId);

        if (subscription == null)
        {
            _logger.LogError("Subscription not found: {SubscriptionId}", invoice.SubscriptionId);
            return;
        }

        // Update subscription status
        await _subscriptionService.UpdateSubscriptionStatus(subscription.Id, "past_due");

        _logger.LogWarning("Payment failed for subscription: {SubscriptionId}", invoice.SubscriptionId);

        // TODO: Implement logic to notify the user about the failed payment
    }
}
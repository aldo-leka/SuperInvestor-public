using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using SuperInvestor.Features.Common.Services;
using SuperInvestor.Features.Identity.Services;
using SuperInvestor.Features.Subscription.Services;
using SubscriptionService = Stripe.SubscriptionService;

namespace SuperInvestor.Features.Subscription.Controllers;

/// <summary>
/// https://docs.stripe.com/billing/subscriptions/build-subscriptions
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class StripeWebhookController(ILogger<StripeWebhookController> logger, ISubscriptionService subscriptionService, IUserService userService)
    : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> HandleWebhook()
    {
        try
        {
            // Read the request body
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            // Verify the webhook signature
            var stripeEvent = ConstructStripeEvent(json);
            if (stripeEvent == null)
            {
                logger.LogWarning("Invalid webhook signature");
                return BadRequest("Invalid signature");
            }
            
            await HandleStripeEvent(stripeEvent);

            return Ok();
        }
        catch (StripeException ex)
        {
            logger.LogError(ex, "Stripe error processing webhook");
            return BadRequest($"Stripe error: {ex.Message}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing Stripe webhook");
            return StatusCode(500, "Internal server error");
        }
    }

    private Event? ConstructStripeEvent(string json)
    {
        try
        {
            var stripeSignature = Request.Headers["Stripe-Signature"];
            return EventUtility.ConstructEvent(json, stripeSignature, EnvironmentHelper.StripeWebHookSecret);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to construct Stripe event");
            return null;
        }
    }

    private async Task HandleStripeEvent(Event stripeEvent)
    {
        switch (stripeEvent.Type)
        {
            case EventTypes.CheckoutSessionCompleted:
                await HandleCheckoutSessionCompleted(stripeEvent);
                break;

            case EventTypes.InvoicePaymentSucceeded:
                await HandleInvoicePaid(stripeEvent);
                break;

            case EventTypes.InvoicePaymentFailed:
                await HandleInvoicePaymentFailed(stripeEvent);
                break;

            case EventTypes.CustomerSubscriptionUpdated:
                await HandleSubscriptionUpdated(stripeEvent);
                break;

            case EventTypes.CustomerSubscriptionDeleted:
                await HandleSubscriptionDeleted(stripeEvent);
                break;
        }
    }

    private async Task HandleCheckoutSessionCompleted(Event stripeEvent)
    {
        var session = stripeEvent.Data.Object as Session;
        LogStripeObject(EventTypes.CheckoutSessionCompleted, session);

        try
        {
            // Get user ID from client_reference_id
            if (string.IsNullOrEmpty(session.ClientReferenceId))
            {
                logger.LogWarning("No client_reference_id found in session {SessionId},", session.Id);
                return;
            }

            var userId = session.ClientReferenceId;
            var customerId = session.CustomerId;

            // Update user with Stripe customer ID
            await userService.UpdateStripeCustomerIdAsync(userId, customerId);

            // If this was a subscription checkout, handle the subscription
            if (!string.IsNullOrEmpty(session.SubscriptionId))
            {
                var stripeSubscriptionService = new SubscriptionService();
                var subscription = await stripeSubscriptionService.GetAsync(session.SubscriptionId);

                await subscriptionService.CreateOrUpdateSubscriptionAsync(userId, subscription);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling checkout.session.completed for session {SessionId}", session.Id);
            throw;
        }
    }

    private async Task HandleInvoicePaid(Event stripeEvent)
    {
        var invoice = stripeEvent.Data.Object as Invoice;
        LogStripeObject(EventTypes.InvoicePaid, invoice);

        try
        {
            var customerId = invoice.CustomerId;
            var user = await userService.GetByStripeCustomerIdAsync(customerId);

            if (user == null)
            {
                logger.LogWarning("No user found for Stripe customer {CustomerId}", customerId);
                return;
            }

            // Update invoice payment status
            await subscriptionService.HandleInvoicePaidAsync(user.Id, invoice);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling invoice.payment_succeeded for invoice {InvoiceId}", invoice?.Id);
            throw;
        }
    }

    private async Task HandleInvoicePaymentFailed(Event stripeEvent)
    {
        var invoice = stripeEvent.Data.Object as Invoice;
        LogStripeObject(EventTypes.InvoicePaymentFailed, invoice);

        try
        {
            var customerId = invoice.CustomerId;
            var user = await userService.GetByStripeCustomerIdAsync(customerId);

            if (user == null)
            {
                logger.LogWarning("No user found for Stripe customer {CustomerId}", customerId);
                return;
            }

            await subscriptionService.HandleInvoicePaymentFailedAsync(user.Id, invoice);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling invoice.payment_failed for invoice {InvoiceId}", invoice.Id);
            throw;
        }
    }

    private async Task HandleSubscriptionUpdated(Event stripeEvent)
    {
        var subscription = stripeEvent.Data.Object as Stripe.Subscription;
        LogStripeObject(EventTypes.CustomerSubscriptionUpdated, subscription);

        try
        {
            var customerId = subscription.CustomerId;
            var user = await userService.GetByStripeCustomerIdAsync(customerId);

            if (user == null)
            {
                logger.LogWarning("No user found for Stripe customer {CustomerId}", customerId);
                return;
            }

            await subscriptionService.UpdateSubscriptionAsync(user.Id, subscription);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling customer.subscription.updated for subscription {SubscriptionId}", subscription.Id);
            throw;
        }
    }

    private async Task HandleSubscriptionDeleted(Event stripeEvent)
    {
        var subscription = stripeEvent.Data.Object as Stripe.Subscription;
        LogStripeObject(EventTypes.CustomerSubscriptionDeleted, subscription);

        try
        {
            var customerId = subscription.CustomerId;
            var user = await userService.GetByStripeCustomerIdAsync(customerId);

            if (user == null)
            {
                logger.LogWarning("No user found for Stripe customer {CustomerId}", customerId);
                return;
            }

            await subscriptionService.CancelSubscriptionAsync(user.Id, subscription);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling customer.subscription.deleted for subscription {SubscriptionId}", subscription.Id);
            throw;
        }
    }
    
    private void LogStripeObject(string eventType, object stripeObject)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(stripeObject, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });
        
        logger.LogDebug("Stripe {EventType} structure:\n{Json}", eventType, json);
    }
}
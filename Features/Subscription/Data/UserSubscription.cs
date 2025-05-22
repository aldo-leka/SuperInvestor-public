using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SuperInvestor.Features.Identity.Data;

namespace SuperInvestor.Features.Subscription.Data;

public class UserSubscription
{
    [Key]
    public string Id { get; set; } = string.Empty;

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public string StripeSubscriptionId { get; set; } = string.Empty;

    [Required]
    public string StripeCustomerId { get; set; } = string.Empty;

    public string? PriceId { get; set; }

    public long Quantity { get; set; } = 1;

    [Required]
    public SubscriptionStatus Status { get; set; }

    public DateTime CurrentPeriodStart { get; set; }
    public DateTime CurrentPeriodEnd { get; set; }

    public DateTime? CanceledAt { get; set; }
    public DateTime? TrialEnd { get; set; }

    // Payment failure tracking
    public bool PastDue { get; set; } = false;
    public int? PaymentFailedCount { get; set; } = 0;
    public DateTime? LastPaymentFailure { get; set; }
    public string? LastInvoiceId { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ApplicationUser User { get; set; } = null!;
}
namespace SuperInvestor.Data;

public class Subscription
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public string StripeCustomerId { get; set; }
    public string StripeSubscriptionId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; }
    public string PlanId { get; set; }
    public DateTime? CurrentPeriodStart { get; set; }
    public DateTime? CurrentPeriodEnd { get; set; }
    public string PlanName { get; set; }
    public decimal PlanAmount { get; set; }
    public string PlanCurrency { get; set; }
    public string PlanInterval { get; set; }

    public ApplicationUser User { get; set; }
}
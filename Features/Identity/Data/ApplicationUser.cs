using Microsoft.AspNetCore.Identity;
using SuperInvestor.Features.Notes.Data;
using SuperInvestor.Features.Subscription.Data;

namespace SuperInvestor.Features.Identity.Data;

public class ApplicationUser : IdentityUser
{
    public string Name { get; set; } = string.Empty;
    
    public string? StripeCustomerId { get; set; }

    public virtual ICollection<Note> Notes { get; set; }

    public UserSubscription? Subscription { get; set; }

    public virtual ICollection<Research.Data.Research> Researches { get; set; }

    public ApplicationUser()
    {
        Notes = [];
        Researches = [];
    }
}
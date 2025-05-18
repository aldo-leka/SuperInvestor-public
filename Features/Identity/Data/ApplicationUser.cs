using Microsoft.AspNetCore.Identity;
using SuperInvestor.Features.Notes.Data;
using SuperInvestor.Features.Research.Data;
using SuperInvestor.Features.Subscription.Data;

namespace SuperInvestor.Features.Identity.Data;

public class ApplicationUser : IdentityUser
{
    public string Name { get; set; } = string.Empty;

    public virtual ICollection<Note> Notes { get; set; }

    public SuperInvestor.Features.Subscription.Data.Subscription Subscription { get; set; }

    public virtual ICollection<SuperInvestor.Features.Research.Data.Research> Researches { get; set; }

    public ApplicationUser()
    {
        Notes = [];
        Researches = [];
    }
}
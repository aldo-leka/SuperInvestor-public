using Microsoft.AspNetCore.Identity;

namespace SuperInvestor.Data;

public class ApplicationUser : IdentityUser
{
    public string Name { get; set; }

    public virtual ICollection<Note> Notes { get; set; }

    public Subscription Subscription { get; set; }

    public virtual ICollection<Research> Researches { get; set; }

    public ApplicationUser()
    {
        Notes = [];
        Researches = [];
    }
}
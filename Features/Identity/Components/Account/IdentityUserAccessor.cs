using Microsoft.AspNetCore.Identity;
using SuperInvestor.Features.Identity.Data;
using SuperInvestor.Features.Identity.Services;

namespace SuperInvestor.Features.Identity.Components.Account
{
    internal sealed class IdentityUserAccessor(UserService userService, IdentityRedirectManager redirectManager)
    {
        public async Task<ApplicationUser> GetRequiredUserAsync(HttpContext context)
        {
            var user = await userService.GetUser();

            if (user is null)
            {
                redirectManager.RedirectToWithStatus("Account/InvalidUser", $"Error: Unable to load user.", context);
            }

            return user;
        }
    }
}

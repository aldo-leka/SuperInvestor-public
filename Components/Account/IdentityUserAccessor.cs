using Microsoft.AspNetCore.Identity;
using SuperInvestor.Data;
using SuperInvestor.Services;

namespace SuperInvestor.Components.Account
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

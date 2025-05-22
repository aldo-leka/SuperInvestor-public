using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using SuperInvestor.Features.Identity.Data;
using SuperInvestor.Features.Identity.Services;

namespace SuperInvestor.Features.Identity.Components.Account
{
    internal sealed class IdentityUserAccessor(IUserService userService, AuthenticationStateProvider authStateProvider, IdentityRedirectManager redirectManager)
    {
        public async Task<ApplicationUser> GetRequiredUserAsync(HttpContext context)
        {
            var authState = await authStateProvider.GetAuthenticationStateAsync();
            if (authState.User.Identity.IsAuthenticated)
            {
                var userId = authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    var user = await userService.GetByIdAsync(userId);
                    if (user != null)
                    {
                        return user;
                    }
                }
            }

            redirectManager.RedirectToWithStatus("Account/InvalidUser", $"Error: Unable to load user.", context);

            return null;
        }
    }
}

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using SuperInvestor.Features.Identity.Data;
using System.Security.Claims;

namespace SuperInvestor.Features.Identity.Services
{
    internal sealed class IdentityUserAccessor(AuthenticationStateProvider authenticationStateProvider, IUserService userService, IdentityRedirectManager redirectManager)
    {
        public async Task<ApplicationUser> GetRequiredUserAsync(HttpContext context)
        {
            var auth = await authenticationStateProvider.GetAuthenticationStateAsync();
            if (auth.User.Identity.IsAuthenticated)
            {
                var userId = auth.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
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

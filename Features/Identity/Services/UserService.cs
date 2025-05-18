using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using SuperInvestor.Features.Common.Data;
using SuperInvestor.Features.Identity.Data;

namespace SuperInvestor.Features.Identity.Services;

public class UserService(UserManager<ApplicationUser> userManager, AuthenticationStateProvider authStateProvider, IDbContextFactory<ApplicationDbContext> factory, RoleManager<IdentityRole> roleManager)
{
    private ApplicationUser _cachedUser;
    
    private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    public async Task<ApplicationUser> GetUser()
    {
        await _semaphore.WaitAsync();
        try
        {
            if (_cachedUser != null)
            {
                return _cachedUser;
            }

            var authState = await authStateProvider.GetAuthenticationStateAsync();
            if (authState.User.Identity.IsAuthenticated)
            {
                _cachedUser = await userManager.GetUserAsync(authState.User);
            }

            return _cachedUser;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<ApplicationUser> GetUser(string userId)
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.Users.FindAsync(userId);
    }

    public async Task<ApplicationUser> GetUserByEmailAsync(string email)
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == email.ToUpper());
    }

    public async Task<List<ApplicationUser>> GetAllUsers()
    {
        await _semaphore.WaitAsync();
        try
        {
            return await userManager.Users
                .OrderBy(u => u.UserName)
                .ToListAsync();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<List<ApplicationUser>> GetAllUsers(int pageNumber = 1, int pageSize = 50)
    {
        return await userManager.Users
            .OrderBy(u => u.UserName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task CreateAdminRole()
    {
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }
    }

    public async Task MakeUserAdmin(string email)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user != null)
        {
            await CreateAdminRole();
            await userManager.AddToRoleAsync(user, "Admin");
        }
    }

    public async Task<bool> IsUserAdmin(ApplicationUser user)
    {
        return await userManager.IsInRoleAsync(user, "Admin");
    }
}
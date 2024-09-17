using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Components.Authorization;
using SuperInvestor.Data;
using Microsoft.EntityFrameworkCore;

namespace SuperInvestor.Services;

public class UserService(UserManager<ApplicationUser> userManager, AuthenticationStateProvider authStateProvider, ApplicationDbContext dbContext, RoleManager<IdentityRole> roleManager)
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly AuthenticationStateProvider _authStateProvider = authStateProvider;
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly RoleManager<IdentityRole> _roleManager = roleManager;
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

            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            if (authState.User.Identity.IsAuthenticated)
            {
                _cachedUser = await _userManager.GetUserAsync(authState.User);
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
        await _semaphore.WaitAsync();
        try
        {
            return await _dbContext.Users.FindAsync(userId);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<ApplicationUser> GetUserByEmailAsync(string email)
    {
        await _semaphore.WaitAsync();
        try
        {
            return await _dbContext.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == email.ToUpper());
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<List<ApplicationUser>> GetAllUsers()
    {
        await _semaphore.WaitAsync();
        try
        {
            return await _userManager.Users
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
        await _semaphore.WaitAsync();
        try
        {
            return await _userManager.Users
                .OrderBy(u => u.UserName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task CreateAdminRole()
    {
        if (!await _roleManager.RoleExistsAsync("Admin"))
        {
            await _roleManager.CreateAsync(new IdentityRole("Admin"));
        }
    }

    public async Task MakeUserAdmin(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user != null)
        {
            await CreateAdminRole();
            await _userManager.AddToRoleAsync(user, "Admin");
        }
    }

    public async Task<bool> IsUserAdmin(ApplicationUser user)
    {
        await _semaphore.WaitAsync();
        try
        {
            return await _userManager.IsInRoleAsync(user, "Admin");
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
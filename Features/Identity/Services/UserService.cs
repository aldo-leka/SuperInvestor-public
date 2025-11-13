using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SuperInvestor.Features.Common.Data;
using SuperInvestor.Features.Identity.Data;
using SuperInvestor.Features.Subscription.Data;

namespace SuperInvestor.Features.Identity.Services;

public class UserService(IDbContextFactory<ApplicationDbContext> factory, ILogger<UserService> logger) : IUserService
{
    public async Task<ApplicationUser?> GetByIdAsync(string userId)
    {
        try
        {
            await using var db = await factory.CreateDbContextAsync();
            var user = await db.Users
                .Include(u => u.Subscription)
                .FirstOrDefaultAsync(u => u.Id == userId);

            return user;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving user by ID: {UserId}", userId);
            throw;
        }
    }

    public async Task<ApplicationUser?> GetByEmailAsync(string email)
    {
        try
        {
            await using var db = await factory.CreateDbContextAsync();
            var user = await db.Users
                .Include(u => u.Subscription) 
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

            return user;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving user by email: {Email}", email);
            throw;
        }
    }

    public async Task<List<ApplicationUser>> GetAllAsync(int pageSize = 100, int pageNumber = 1)
    {
        try
        {
            await using var db = await factory.CreateDbContextAsync();
            return await db.Users
                .Include(u => u.Subscription)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving all users");
            throw;
        }
    }

    public async Task<bool> CreateAdminRoleAsync()
    {
        try
        {
            await using var db = await factory.CreateDbContextAsync();
            var existingRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            if (existingRole != null) return true;

            var adminRole = new IdentityRole("Admin")
            {
                NormalizedName = "ADMIN"
            };

            db.Roles.Add(adminRole);
            await db.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating admin role");
            throw;
        }
    }

    public async Task<bool> MakeUserAdminAsync(string email)
    {
        try
        {
            var user = await GetByEmailAsync(email);

            if (user == null)
            {
                logger.LogWarning("User with email {Email} not found", email);
                return false;
            }

            await using var checkDb = await factory.CreateDbContextAsync();
            var adminRoleExists = await checkDb.Roles.AnyAsync(r => r.Name == "Admin");
            if (!adminRoleExists)
            {
                var roleResult = await CreateAdminRoleAsync();
                if (!roleResult)
                {
                    return false;
                }
            }

            if (await IsUserAdminAsync(user))
            {
                return true;
            }

            await using var db = await factory.CreateDbContextAsync();
            var adminRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            if (adminRole == null)
            {
                logger.LogError("Admin role not found");
                return false;
            }

            var userRole = new IdentityUserRole<string>
            {
                UserId = user.Id,
                RoleId = adminRole.Id
            };

            db.UserRoles.Add(userRole);
            await db.SaveChangesAsync();
            return true;

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error assigning admin role to user {Email}", email);
            throw;
        }
    }

    public async Task<bool> IsUserAdminAsync(ApplicationUser user)
    {
        try
        {
            await using var db = await factory.CreateDbContextAsync();
            var userRoles = await db.UserRoles
                .Where(ur => ur.UserId == user.Id)
                .Join(db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                .ToListAsync();
            
            return userRoles.Contains("Admin");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking admin status for user {UserId}", user.Id);
            throw;
        }
    }

    public async Task<int> GetUserCountAsync()
    {
        try
        {
            await using var db = await factory.CreateDbContextAsync();
            return await db.Users.CountAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting user count");
            throw;
        }
    }
    
    public async Task<bool> UserHasActiveSubscriptionAsync(string userId)
    {
        try
        {
            var user = await GetByIdAsync(userId);
            if (user != null && await IsUserAdminAsync(user))
            {
                return true;
            }

            await using var db = await factory.CreateDbContextAsync();
            var hasActiveSubscription = await db.UserSubscriptions
                .AnyAsync(s => s.UserId == userId &&
                               (s.Status == SubscriptionStatus.Active ||
                                s.Status == SubscriptionStatus.Trialing) &&
                               s.CurrentPeriodEnd > DateTime.UtcNow);

            return hasActiveSubscription;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking subscription status for user {UserId}", userId);
            throw;
        }
    }
    
    public async Task<ApplicationUser?> GetByStripeCustomerIdAsync(string customerId)
    {
        try
        {
            await using var db = await factory.CreateDbContextAsync();
            var user = await db.Users
                .Include(u => u.Subscription)
                .FirstOrDefaultAsync(u => u.StripeCustomerId == customerId);

            return user;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving user by Stripe customer ID: {CustomerId}", customerId);
            throw;
        }
    }
    
    public async Task<bool> UpdateStripeCustomerIdAsync(string userId, string customerId)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
            {
                logger.LogError("Cannot update Stripe customer ID: User ID is null or empty");
                return false;
            }

            if (string.IsNullOrEmpty(customerId))
            {
                logger.LogError("Cannot update Stripe customer ID: Customer ID is null or empty");
                return false;
            }

            var user = await GetByIdAsync(userId);
            if (user == null)
            {
                logger.LogError("User not found with ID: {UserId}", userId);
                return false;
            }

            // Check if the user already has this Stripe customer ID
            if (user.StripeCustomerId == customerId)
            {
                return true;
            }

            user.StripeCustomerId = customerId;

            await using var db = await factory.CreateDbContextAsync();
            db.Users.Update(user);
            await db.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating Stripe customer ID for user {UserId}", userId);
            throw;
        }
    }
}
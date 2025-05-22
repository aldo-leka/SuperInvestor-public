using SuperInvestor.Features.Identity.Data;

namespace SuperInvestor.Features.Identity.Services;

public interface IUserService
{
    Task<ApplicationUser?> GetByIdAsync(string userId);
    Task<ApplicationUser?> GetByEmailAsync(string email);
    Task<List<ApplicationUser>> GetAllAsync(int pageSize = 100, int pageNumber = 1);
    Task<bool> CreateAdminRoleAsync();
    Task<bool> MakeUserAdminAsync(string email);
    Task<bool> IsUserAdminAsync(ApplicationUser user);
    Task<int> GetUserCountAsync();
    Task<bool> UserHasActiveSubscriptionAsync(string userId);
    Task<ApplicationUser?> GetByStripeCustomerIdAsync(string customerId);
    Task<bool> UpdateStripeCustomerIdAsync(string userId, string customerId);
}
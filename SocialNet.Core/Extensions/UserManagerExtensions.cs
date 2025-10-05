using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SocialNet.Domain.Identity;

namespace SocialNet.Core.Extensions;

public static class UserManagerExtensions
{
    public static async Task<User> GetUserByIdAsync(this UserManager<User> userManager, int userId)
    {
        var query = userManager.Users;

        return await query.FirstOrDefaultAsync(u => u.Id == userId);
    }
}

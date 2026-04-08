#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Data.Repositories;

using DotnetMicroOrm.Domain.Models;

/// <summary>
/// User-specific repository with extended operations
/// </summary>
public class sealed UserRepository : Repository<User>
{
    public UserRepository(IDatabaseContext context) : base(context) { }

    // Gets user by username
    public async Task<User?> GetByUsernameAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return null;

        var users = await GetAllAsync();
        return users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
    }

    // Gets user by email
    public async Task<User?> GetByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        var users = await GetAllAsync();
        return users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
    }

    // Gets active users
    public async Task<List<User>> GetActiveUsersAsync()
    {
        var users = await GetAllAsync();
        return users.Where(u => u.IsActive).ToList();
    }

    // Gets verified users
    public async Task<List<User>> GetVerifiedUsersAsync()
    {
        var users = await GetAllAsync();
        return users.Where(u => u.IsEmailVerified && u.IsActive).ToList();
    }

    // Gets users by creation date range
    public async Task<List<User>> GetUsersByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var users = await GetAllAsync();
        return users.Where(u => u.CreatedDate >= startDate && u.CreatedDate <= endDate).ToList();
    }

    // Gets users without recent login
    public async Task<List<User>> GetInactiveUsersAsync(int daysInactive = 30)
    {
        var users = await GetActiveUsersAsync();
        var cutoffDate = DateTime.UtcNow.AddDays(-daysInactive);
        return users.Where(u => !u.LastLoginDate.HasValue || u.LastLoginDate < cutoffDate).ToList();
    }

    // Counts active users
    public async Task<int> CountActiveUsersAsync()
    {
        var users = await GetActiveUsersAsync();
        return users.Count;
    }
}

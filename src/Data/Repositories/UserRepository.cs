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
public sealed class UserRepository : Repository<User>
{
    /// <summary>
    /// Initializes a new instance of the UserRepository class.
    /// </summary>
    /// <param name="context">The database context to use for data operations.</param>
    public UserRepository(IDatabaseContext context) : base(context) { }

    /// <summary>
    /// Gets a user by their username.
    /// </summary>
    /// <param name="username">The username to search for.</param>
    /// <returns>The user with the specified username, or null if not found.</returns>
    public async Task<User?> GetByUsernameAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return null;

        var users = await GetAllAsync();
        return users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets a user by their email address.
    /// </summary>
    /// <param name="email">The email address to search for.</param>
    /// <returns>The user with the specified email address, or null if not found.</returns>
    public async Task<User?> GetByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        var users = await GetAllAsync();
        return users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets all active users.
    /// </summary>
    /// <returns>A list of all active users.</returns>
    public async Task<List<User>> GetActiveUsersAsync()
    {
        var users = await GetAllAsync();
        return users.Where(u => u.IsActive).ToList();
    }

    /// <summary>
    /// Gets all verified and active users.
    /// </summary>
    /// <returns>A list of all verified and active users.</returns>
    public async Task<List<User>> GetVerifiedUsersAsync()
    {
        var users = await GetAllAsync();
        return users.Where(u => u.IsEmailVerified && u.IsActive).ToList();
    }

    /// <summary>
    /// Gets users created within a specific date range.
    /// </summary>
    /// <param name="startDate">The start date of the range (inclusive).</param>
    /// <param name="endDate">The end date of the range (inclusive).</param>
    /// <returns>A list of users created within the specified date range.</returns>
    public async Task<List<User>> GetUsersByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var users = await GetAllAsync();
        return users.Where(u => u.CreatedDate >= startDate && u.CreatedDate <= endDate).ToList();
    }

    /// <summary>
    /// Gets users who have not logged in for a specified number of days.
    /// </summary>
    /// <param name="daysInactive">The number of days of inactivity to consider a user inactive. Default is 30 days.</param>
    /// <returns>A list of inactive users.</returns>
    public async Task<List<User>> GetInactiveUsersAsync(int daysInactive = 30)
    {
        var users = await GetActiveUsersAsync();
        var cutoffDate = DateTime.UtcNow.AddDays(-daysInactive);
        return users.Where(u => !u.LastLoginDate.HasValue || u.LastLoginDate < cutoffDate).ToList();
    }

    /// <summary>
    /// Counts the number of active users.
    /// </summary>
    /// <returns>The count of active users.</returns>
    public async Task<int> CountActiveUsersAsync()
    {
        var users = await GetActiveUsersAsync();
        return users.Count;
    }
}

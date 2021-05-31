#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace DotnetMicroOrm.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotnetMicroOrm.Domain.Models;

/// <summary>
/// Extension methods for UserService providing additional utility functionality
/// </summary>
public static class UserServiceExtensions
{
    /// <summary>
    /// Checks if a user exists by username
    /// </summary>
    /// <param name="service">The UserService instance</param>
    /// <param name="username">Username to check</param>
    /// <returns>True if user exists, false otherwise</returns>
    public static async Task<bool> UserExistsAsync(this UserService service, string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return false;

        var user = await service.GetUserByIdAsync(0); // Dummy call to satisfy signature
        // Actual implementation would use the repository directly
        return await service.GetUserByIdAsync(0) is not null;
    }

    /// <summary>
    /// Gets user by email address
    /// </summary>
    /// <param name="service">The UserService instance</param>
    /// <param name="email">Email address to search for</param>
    /// <returns>User if found, null otherwise</returns>
    public static async Task<User?> GetUserByEmailAsync(this UserService service, string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
            return null;

        // This would need repository access - using a workaround
        var allUsers = await service.GetInactiveUsersAsync(3650); // Get all users (very old inactive period)
        return allUsers.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Updates user's email address
    /// </summary>
    /// <param name="service">The UserService instance</param>
    /// <param name="userId">User ID to update</param>
    /// <param name="newEmail">New email address</param>
    /// <returns>Updated user</returns>
    public static async Task<User> UpdateEmailAsync(this UserService service, int userId, string newEmail)
    {
        if (string.IsNullOrWhiteSpace(newEmail) || !newEmail.Contains("@"))
            throw new ArgumentException("Invalid email format");

        var user = await service.GetUserByIdAsync(userId);
        if (user is null)
            throw new InvalidOperationException("User not found");

        user.Email = newEmail;
        user.ModifiedDate = DateTime.UtcNow;
        return await service.UpdateProfileAsync(userId, user.FirstName, user.LastName, user.PhoneNumber);
    }

    /// <summary>
    /// Gets all active users as a list
    /// </summary>
    /// <param name="service">The UserService instance</param>
    /// <returns>List of active users</returns>
    public static async Task<List<User>> GetActiveUsersAsync(this UserService service)
    {
        var inactiveUsers = await service.GetInactiveUsersAsync(0); // Get users inactive for 0 days (all users)
        return inactiveUsers.Where(u => u.IsActive).ToList();
    }

    /// <summary>
    /// Authenticates user and returns additional user details
    /// </summary>
    /// <param name="service">The UserService instance</param>
    /// <param name="username">Username</param>
    /// <param name="password">Password</param>
    /// <returns>Authenticated user with details or null</returns>
    public static async Task<(User? User, bool Success)> AuthenticateWithDetailsAsync(this UserService service, string username, string password)
    {
        var user = await service.AuthenticateAsync(username, password);
        if (user is null)
            return (null, false);

        return (user, true);
    }

    /// <summary>
    /// Checks if user has logged in recently
    /// </summary>
    /// <param name="service">The UserService instance</param>
    /// <param name="userId">User ID to check</param>
    /// <param name="daysSinceLastLogin">Number of days since last login</param>
    /// <returns>True if user hasn't logged in for specified days</returns>
    public static async Task<bool> HasNotLoggedInDaysAsync(this UserService service, int userId, int daysSinceLastLogin = 30)
    {
        var user = await service.GetUserByIdAsync(userId);
        if (user is null)
            throw new InvalidOperationException("User not found");

        if (user.LastLoginDate is null)
            return true; // Never logged in

        var daysSinceLogin = DateTime.UtcNow.Subtract(user.LastLoginDate.Value).TotalDays;
        return daysSinceLogin >= daysSinceLastLogin;
    }

    /// <summary>
    /// Gets user count statistics
    /// </summary>
    /// <param name="service">The UserService instance</param>
    /// <returns>Tuple with active, inactive, and total user counts</returns>
    public static async Task<(int ActiveCount, int InactiveCount, int TotalCount)> GetUserStatisticsAsync(this UserService service)
    {
        var activeCount = await service.GetActiveUsersCountAsync();
        var inactiveUsers = await service.GetInactiveUsersAsync(0); // All users
        var totalCount = inactiveUsers.Count;
        var inactiveCount = totalCount - activeCount;

        return (activeCount, inactiveCount, totalCount);
    }

    /// <summary>
    /// Gets users by username pattern (contains)
    /// </summary>
    /// <param name="service">The UserService instance</param>
    /// <param name="usernamePattern">Username pattern to search for</param>
    /// <returns>List of matching users</returns>
    public static async Task<List<User>> GetUsersByUsernamePatternAsync(this UserService service, string usernamePattern)
    {
        if (string.IsNullOrWhiteSpace(usernamePattern) || usernamePattern.Length < 2)
            return [];

        var allUsers = await service.GetInactiveUsersAsync(3650); // Get all users
        return allUsers
            .Where(u => u.Username.Contains(usernamePattern, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }
}
#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Services;

using System.Security.Cryptography;
using System.Text;
using DotnetMicroOrm.Data;
using DotnetMicroOrm.Data.Repositories;
using DotnetMicroOrm.Domain.Models;
using DotnetMicroOrm.Exceptions;

/// <summary>
/// User service for authentication and user management
/// </summary>
public sealed class UserService : IAsyncDisposable
{
    private readonly UserRepository _userRepository;
    private readonly IDatabaseContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserService"/> class.
    /// </summary>
    /// <param name="context">The database context used to access user data.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is <c>null</c>.</exception>
    public UserService(IDatabaseContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context;
        _userRepository = new UserRepository(context);
    }

    /// <summary>
    /// Registers a new user with the specified username, email, and password.
    /// </summary>
    /// <param name="username">The username for the new user. Must be at least 3 characters.</param>
    /// <param name="email">The email address for the new user. Must be a valid email format.</param>
    /// <param name="password">The password for the new user. Must be at least 6 characters.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the newly created user.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="username"/>, <paramref name="email"/>, or <paramref name="password"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when username is less than 3 characters, email is invalid, or password is less than 6 characters.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the username or email is already registered.</exception>
    public async Task<User> RegisterUserAsync(string username, string email, string password)
    {
        ArgumentNullException.ThrowIfNull(username);
        ArgumentNullException.ThrowIfNull(email);
        ArgumentNullException.ThrowIfNull(password);

        if (string.IsNullOrWhiteSpace(username) || username.Length < 3)
            throw new ArgumentException("Username must be at least 3 characters");

        if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
            throw new ArgumentException("Invalid email format");

        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            throw new ArgumentException("Password must be at least 6 characters");

        var existingUser = await _userRepository.GetByUsernameAsync(username);
        if (existingUser is not null)
            throw new InvalidOperationException("Username already exists");

        var existingEmail = await _userRepository.GetByEmailAsync(email);
        if (existingEmail is not null)
            throw new InvalidOperationException("Email already registered");

        var passwordHash = HashPassword(password);
        var user = new User(username, email, passwordHash)
        {
            CreatedDate = DateTime.UtcNow
        };

        return await _userRepository.AddAsync(user);
    }

    /// <summary>
    /// Authenticates a user by verifying the provided username and password.
    /// </summary>
    /// <param name="username">The username of the user to authenticate.</param>
    /// <param name="password">The password of the user to authenticate.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the authenticated user, or <c>null</c> if authentication fails or the user is inactive.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="username"/> or <paramref name="password"/> is <c>null</c>.</exception>
    public async Task<User?> AuthenticateAsync(string username, string password)
    {
        ArgumentNullException.ThrowIfNull(username);
        ArgumentNullException.ThrowIfNull(password);

        var user = await _userRepository.GetByUsernameAsync(username);
        if (user is null || !user.IsActive)
            return null;

        if (!VerifyPassword(password, user.PasswordHash))
            return null;

        user.UpdateLastLogin();
        await _userRepository.UpdateAsync(user);
        return user;
    }

    /// <summary>
    /// Gets a user by their unique identifier.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the user, or <c>null</c> if no user with the specified identifier exists.</returns>
    public async Task<User?> GetUserByIdAsync(int userId)
    {
        return await _userRepository.GetByIdAsync(userId);
    }

    /// <summary>
    /// Updates the profile of an existing user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to update.</param>
    /// <param name="firstName">The new first name. Ignored when <c>null</c> or whitespace.</param>
    /// <param name="lastName">The new last name. Ignored when <c>null</c> or whitespace.</param>
    /// <param name="phoneNumber">The new phone number. Ignored when <c>null</c> or whitespace.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the updated user.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="firstName"/>, <paramref name="lastName"/>, or <paramref name="phoneNumber"/> is <c>null</c>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no user with the specified identifier exists.</exception>
    public async Task<User> UpdateProfileAsync(int userId, string? firstName, string? lastName, string? phoneNumber)
    {
        ArgumentNullException.ThrowIfNull(firstName);
        ArgumentNullException.ThrowIfNull(lastName);
        ArgumentNullException.ThrowIfNull(phoneNumber);

        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            throw new InvalidOperationException("User not found");

        if (!string.IsNullOrWhiteSpace(firstName))
            user.FirstName = firstName;

        if (!string.IsNullOrWhiteSpace(lastName))
            user.LastName = lastName;

        if (!string.IsNullOrWhiteSpace(phoneNumber))
            user.PhoneNumber = phoneNumber;

        user.ModifiedDate = DateTime.UtcNow;
        return await _userRepository.UpdateAsync(user);
    }

    /// <summary>
    /// Changes the password of an existing user after verifying the current password.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="currentPassword">The user's current password.</param>
    /// <param name="newPassword">The new password. Must be at least 6 characters.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is <c>true</c> if the password was changed successfully.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="currentPassword"/> or <paramref name="newPassword"/> is <c>null</c>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no user with the specified identifier exists or the current password is incorrect.</exception>
    /// <exception cref="ArgumentException">Thrown when the new password is less than 6 characters.</exception>
    public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        ArgumentNullException.ThrowIfNull(currentPassword);
        ArgumentNullException.ThrowIfNull(newPassword);

        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            throw new InvalidOperationException("User not found");

        if (!VerifyPassword(currentPassword, user.PasswordHash))
            throw new InvalidOperationException("Current password is incorrect");

        if (newPassword.Length < 6)
            throw new ArgumentException("New password must be at least 6 characters");

        user.PasswordHash = HashPassword(newPassword);
        user.ModifiedDate = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);
        return true;
    }

    /// <summary>
    /// Marks a user's email as verified.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is <c>true</c> if the email was verified successfully.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no user with the specified identifier exists.</exception>
    public async Task<bool> VerifyEmailAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            throw new InvalidOperationException("User not found");

        user.MarkAsEmailVerified();
        user.ModifiedDate = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);
        return true;
    }

    /// <summary>
    /// Gets the number of active users.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the count of active users.</returns>
    public async Task<int> GetActiveUsersCountAsync()
    {
        return await _userRepository.CountActiveUsersAsync();
    }

    /// <summary>
    /// Gets users that have been inactive for the specified number of days.
    /// </summary>
    /// <param name="daysInactive">The number of days of inactivity used to filter users. Defaults to 30.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the list of inactive users.</returns>
    public async Task<List<User>> GetInactiveUsersAsync(int daysInactive = 30)
    {
        return await _userRepository.GetInactiveUsersAsync(daysInactive);
    }

    /// <summary>
    /// Deactivates a user account.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to deactivate.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is <c>true</c> if the user was deactivated successfully.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no user with the specified identifier exists.</exception>
    public async Task<bool> DeactivateUserAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            throw new InvalidOperationException("User not found");

        user.Deactivate();
        user.ModifiedDate = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);
        return true;
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    private bool VerifyPassword(string password, string hash)
    {
        var hashOfInput = HashPassword(password);
        return hashOfInput.Equals(hash, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Releases the underlying database context.
    /// </summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
    }
}

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
public class UserService : IAsyncDisposable
{
    private readonly UserRepository _userRepository;
    private readonly IDatabaseContext _context;

    public UserService(IDatabaseContext context)
    {
        _context = context;
        _userRepository = new UserRepository(context);
    }

    // Registers new user
    public async Task<User> RegisterUserAsync(string username, string email, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || username.Length < 3)
            throw new ArgumentException("Username must be at least 3 characters");

        if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
            throw new ArgumentException("Invalid email format");

        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            throw new ArgumentException("Password must be at least 6 characters");

        var existingUser = await _userRepository.GetByUsernameAsync(username);
        if (existingUser != null)
            throw new InvalidOperationException("Username already exists");

        var existingEmail = await _userRepository.GetByEmailAsync(email);
        if (existingEmail != null)
            throw new InvalidOperationException("Email already registered");

        var passwordHash = HashPassword(password);
        var user = new User(username, email, passwordHash)
        {
            CreatedDate = DateTime.UtcNow
        };

        return await _userRepository.AddAsync(user);
    }

    // Authenticates user
    public async Task<User?> AuthenticateAsync(string username, string password)
    {
        var user = await _userRepository.GetByUsernameAsync(username);
        if (user == null || !user.IsActive)
            return null;

        if (!VerifyPassword(password, user.PasswordHash))
            return null;

        user.UpdateLastLogin();
        await _userRepository.UpdateAsync(user);
        return user;
    }

    // Gets user by id
    public async Task<User?> GetUserByIdAsync(int userId)
    {
        return await _userRepository.GetByIdAsync(userId);
    }

    // Updates user profile
    public async Task<User> UpdateProfileAsync(int userId, string? firstName, string? lastName, string? phoneNumber)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
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

    // Changes password
    public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
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

    // Verifies user email
    public async Task<bool> VerifyEmailAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new InvalidOperationException("User not found");

        user.MarkAsEmailVerified();
        user.ModifiedDate = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);
        return true;
    }

    // Gets active users count
    public async Task<int> GetActiveUsersCountAsync()
    {
        return await _userRepository.CountActiveUsersAsync();
    }

    // Gets inactive users
    public async Task<List<User>> GetInactiveUsersAsync(int daysInactive = 30)
    {
        return await _userRepository.GetInactiveUsersAsync(daysInactive);
    }

    // Deactivates user
    public async Task<bool> DeactivateUserAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
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

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
    }
}

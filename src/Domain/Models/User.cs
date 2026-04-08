#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Domain.Models;

/// <summary>
/// Represents a system user entity with authentication and profile information
/// </summary>
[Table("Users")]
public class sealed User : BaseEntity
{
    [Column("Id", IsPrimaryKey = true)]
    public int Id { get; set; }

    [Column("Username", IsNullable = false, MaxLength = 50)]
    public string Username { get; set; } = string.Empty;

    [Column("Email", IsNullable = false, MaxLength = 100)]
    public string Email { get; set; } = string.Empty;

    [Column("PasswordHash", IsNullable = false)]
    public string PasswordHash { get; set; } = string.Empty;

    [Column("FirstName", MaxLength = 50)]
    public string? FirstName { get; set; }

    [Column("LastName", MaxLength = 50)]
    public string? LastName { get; set; }

    [Column("PhoneNumber", MaxLength = 20)]
    public string? PhoneNumber { get; set; }

    [Column("IsActive", IsNullable = false)]
    public bool IsActive { get; set; } = true;

    [Column("IsEmailVerified", IsNullable = false)]
    public bool IsEmailVerified { get; set; }

    [Column("LastLoginDate")]
    public DateTime? LastLoginDate { get; set; }

    [Column("CreatedDate", IsNullable = false)]
    public DateTime CreatedDate { get; set; }

    [Column("ModifiedDate")]
    public DateTime? ModifiedDate { get; set; }

    [Column("Version", IsNullable = false)]
    public int Version { get; set; }

    // Navigation properties
    [NotMapped]
    public virtual List<Order> Orders { get; set; } = [];

    public User() { }

    public User(string username, string email, string passwordHash)
    {
        Username = username;
        Email = email;
        PasswordHash = passwordHash;
        CreatedDate = DateTime.UtcNow;
        Version = 1;
    }

    /// <summary>
    /// Validates user data before persistence
    /// </summary>
    public override bool Validate(out List<string> errors)
    {
        errors = [];

        if (string.IsNullOrWhiteSpace(Username) || Username.Length < 3)
            errors.Add("Username must be at least 3 characters long");

        if (string.IsNullOrWhiteSpace(Email) || !Email.Contains("@"))
            errors.Add("Email must be valid");

        if (string.IsNullOrWhiteSpace(PasswordHash) || PasswordHash.Length < 32)
            errors.Add("Password hash is invalid");

        if (!string.IsNullOrEmpty(FirstName) && FirstName.Length > 50)
            errors.Add("First name cannot exceed 50 characters");

        if (!string.IsNullOrEmpty(LastName) && LastName.Length > 50)
            errors.Add("Last name cannot exceed 50 characters");

        return errors.Count == 0;
    }

    public string GetFullName() => $"{FirstName} {LastName}".Trim();

    public void MarkAsEmailVerified() => IsEmailVerified = true;

    public void UpdateLastLogin() => LastLoginDate = DateTime.UtcNow;

    public void Deactivate() => IsActive = false;
}

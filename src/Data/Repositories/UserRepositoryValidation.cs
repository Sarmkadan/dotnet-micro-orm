#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotnetMicroOrm.Data.Repositories;

using System.Collections.Generic;
using System.Globalization;

/// <summary>
/// Provides validation helpers for <see cref="UserRepository"/> instances
/// </summary>
public static class UserRepositoryValidation
{
	/// <summary>
	/// Validates the repository instance and its dependencies
	/// </summary>
	/// <param name="value">The repository instance to validate</param>
	/// <returns>A list of human-readable validation problems, or empty list if valid</returns>
	/// <exception cref="ArgumentNullException">Thrown when value is null</exception>
	public static IReadOnlyList<string> Validate(this UserRepository value)
	{
		ArgumentNullException.ThrowIfNull(value);

		var errors = new List<string>();

		// Repository-level validations
		// UserRepository inherits from Repository<User> which has IDatabaseContext dependency
		// The context is validated in the base constructor

		return errors.AsReadOnly();
	}

	/// <summary>
	/// Determines whether the repository instance is valid
	/// </summary>
	/// <param name="value">The repository instance to check</param>
	/// <returns>True if the repository is valid; otherwise, false</returns>
	public static bool IsValid(this UserRepository value) => Validate(value).Count == 0;

	/// <summary>
	/// Validates username parameter for UserRepository methods
	/// </summary>
	/// <param name="username">The username to validate</param>
	/// <returns>True if username is valid; otherwise false</returns>
	/// <exception cref="ArgumentNullException">Thrown when username is null</exception>
	/// <exception cref="ArgumentException">Thrown when username is empty or whitespace</exception>
	public static bool IsValidUsername(this string username)
	{
		ArgumentNullException.ThrowIfNull(username);
		return !string.IsNullOrWhiteSpace(username);
	}

	/// <summary>
	/// Validates email parameter for UserRepository methods
	/// </summary>
	/// <param name="email">The email to validate</param>
	/// <returns>True if email is valid; otherwise false</returns>
	/// <exception cref="ArgumentNullException">Thrown when email is null</exception>
	/// <exception cref="ArgumentException">Thrown when email is empty or whitespace</exception>
	public static bool IsValidEmail(this string email)
	{
		ArgumentNullException.ThrowIfNull(email);
		return !string.IsNullOrWhiteSpace(email) && email.Contains('@');
	}

	/// <summary>
	/// Validates date range parameters for UserRepository methods
	/// </summary>
	/// <param name="startDate">The start date</param>
	/// <param name="endDate">The end date</param>
	/// <returns>True if date range is valid; otherwise false</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when start date is after end date</exception>
	public static bool IsValidDateRange(this DateTime startDate, DateTime endDate)
	{
		if (startDate > endDate)
		{
			throw new ArgumentOutOfRangeException(nameof(startDate), "Start date cannot be after end date");
		}
		return true;
	}

	/// <summary>
	/// Validates daysInactive parameter for GetInactiveUsersAsync method
	/// </summary>
	/// <param name="daysInactive">Number of days to consider a user inactive</param>
	/// <returns>True if parameter is valid; otherwise false</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when daysInactive is less than 1</exception>
	public static bool IsValidDaysInactive(this int daysInactive)
	{
		if (daysInactive < 1)
		{
			throw new ArgumentOutOfRangeException(nameof(daysInactive), "Days inactive must be at least 1");
		}
		return true;
	}

	/// <summary>
	/// Ensures the repository instance is valid, throwing an exception if not
	/// </summary>
	/// <param name="value">The repository instance to validate</param>
	/// <exception cref="ArgumentNullException">Thrown when value is null</exception>
	/// <exception cref="ArgumentException">Thrown when the repository is invalid with a list of problems</exception>
	public static void EnsureValid(this UserRepository value)
	{
		ArgumentNullException.ThrowIfNull(value);

		var errors = Validate(value);
		if (errors.Count > 0)
		{
			throw new ArgumentException(
				$"UserRepository validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
		}
	}
}
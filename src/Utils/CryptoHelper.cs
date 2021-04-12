#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Security.Cryptography;
using System.Text;

namespace DotnetMicroOrm.Utils;

/// <summary>
/// Provides cryptographic operations: hashing, encryption, and secure random generation.
/// Uses modern algorithms (PBKDF2, AES-256) with proper salt/IV handling.
/// Critical for password storage and sensitive data protection.
/// </summary>
public static class CryptoHelper
{
    private const int SaltLength = 16;
    private const int HashLength = 32;
    private const int Iterations = 10000;

    /// <summary>
    /// Creates a salted hash of a password using PBKDF2-SHA256
    /// Returns base64-encoded salt + hash for storage in database
    /// </summary>
    public static string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty", nameof(password));

        using (var rng = RandomNumberGenerator.Create())
        {
            var salt = new byte[SaltLength];
            rng.GetBytes(salt);

            var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashLength);
            var hashBytes = new byte[SaltLength + HashLength];

            Buffer.BlockCopy(salt, 0, hashBytes, 0, SaltLength);
            Buffer.BlockCopy(hash, 0, hashBytes, SaltLength, HashLength);

            return Convert.ToBase64String(hashBytes);
        }
    }

    /// <summary>
    /// Verifies a password against a stored hash created by HashPassword
    /// Uses constant-time comparison to prevent timing attacks
    /// </summary>
    public static bool VerifyPassword(string password, string storedHash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(storedHash))
            return false;

        try
        {
            var hashBytes = Convert.FromBase64String(storedHash);

            if (hashBytes.Length != SaltLength + HashLength)
                return false;

            var salt = new byte[SaltLength];
            Buffer.BlockCopy(hashBytes, 0, salt, 0, SaltLength);

            var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashLength);

            // Constant-time comparison to prevent timing attacks
            var storedHashPortion = new byte[HashLength];
            Buffer.BlockCopy(hashBytes, SaltLength, storedHashPortion, 0, HashLength);

            return CryptographicEquals(hash, storedHashPortion);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Generates a cryptographically secure random token suitable for API keys/tokens
    /// </summary>
    public static string GenerateSecureToken(int length = 32)
    {
        if (length < 16)
            throw new ArgumentException("Token length must be at least 16 bytes", nameof(length));

        using (var rng = RandomNumberGenerator.Create())
        {
            var tokenBytes = new byte[length];
            rng.GetBytes(tokenBytes);
            return Convert.ToBase64String(tokenBytes);
        }
    }

    /// <summary>
    /// Creates SHA256 hash of a string (not suitable for passwords, use HashPassword instead)
    /// Used for checksums and data integrity verification
    /// </summary>
    public static string ComputeSha256(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Input cannot be empty", nameof(input));

        using (var sha256 = SHA256.Create())
        {
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(hashBytes);
        }
    }

    /// <summary>
    /// Encrypts a string using AES-256-CBC with random IV
    /// Returns base64-encoded IV + ciphertext
    /// </summary>
    public static string EncryptAes256(string plaintext, string key)
    {
        if (string.IsNullOrEmpty(plaintext))
            throw new ArgumentException("Plaintext cannot be empty", nameof(plaintext));
        if (string.IsNullOrEmpty(key) || key.Length < 32)
            throw new ArgumentException("Key must be at least 32 characters", nameof(key));

        var keyBytes = Encoding.UTF8.GetBytes(key[..32]); // Use first 32 chars as 256-bit key

        using (var aes = Aes.Create())
        {
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.GenerateIV();

            using (var encryptor = aes.CreateEncryptor(keyBytes, aes.IV))
            using (var ms = new MemoryStream())
            {
                ms.Write(aes.IV, 0, aes.IV.Length);

                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    var plainBytes = Encoding.UTF8.GetBytes(plaintext);
                    cs.Write(plainBytes, 0, plainBytes.Length);
                    cs.FlushFinalBlock();
                }

                return Convert.ToBase64String(ms.ToArray());
            }
        }
    }

    /// <summary>
    /// Decrypts an AES-256-CBC encrypted string created by EncryptAes256
    /// </summary>
    public static string DecryptAes256(string ciphertext, string key)
    {
        if (string.IsNullOrEmpty(ciphertext))
            throw new ArgumentException("Ciphertext cannot be empty", nameof(ciphertext));
        if (string.IsNullOrEmpty(key) || key.Length < 32)
            throw new ArgumentException("Key must be at least 32 characters", nameof(key));

        var keyBytes = Encoding.UTF8.GetBytes(key[..32]);
        var ciphertextBytes = Convert.FromBase64String(ciphertext);

        using (var aes = Aes.Create())
        {
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            var iv = new byte[aes.IV.Length];
            Buffer.BlockCopy(ciphertextBytes, 0, iv, 0, iv.Length);

            using (var decryptor = aes.CreateDecryptor(keyBytes, iv))
            using (var ms = new MemoryStream(ciphertextBytes, iv.Length, ciphertextBytes.Length - iv.Length))
            using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
            using (var sr = new StreamReader(cs, Encoding.UTF8))
            {
                return sr.ReadToEnd();
            }
        }
    }

    /// <summary>
    /// Constant-time comparison to prevent timing attacks
    /// </summary>
    private static bool CryptographicEquals(byte[] a, byte[] b)
    {
        if (a.Length != b.Length)
            return false;

        int result = 0;
        for (int i = 0; i < a.Length; i++)
            result |= a[i] ^ b[i];

        return result == 0;
    }
}

namespace IotGrpcLearning.Interfaces;

/// <summary>
/// Provides secure password hashing and verification.
/// </summary>
public interface IPasswordService
{
    /// <summary>
    /// Hashes a password using PBKDF2 with a random salt.
    /// </summary>
    /// <param name="password">The plain-text password to hash.</param>
    /// <returns>A tuple containing the Base64-encoded hash and salt.</returns>
    (string hash, string salt) HashPassword(string password);

    /// <summary>
    /// Verifies a password against a stored hash and salt.
    /// </summary>
    /// <param name="password">The plain-text password to verify.</param>
    /// <param name="storedHash">The stored Base64-encoded hash.</param>
    /// <param name="storedSalt">The stored Base64-encoded salt.</param>
    /// <returns>True if the password matches; otherwise, false.</returns>
    bool VerifyPassword(string password, string storedHash, string storedSalt);

    /// <summary>
    /// Hashes a password and returns it in a metadata format (for future migration).
    /// </summary>
    /// <param name="password">The plain-text password to hash.</param>
    /// <returns>A formatted string containing version, iterations, salt, and hash.</returns>
    string HashPasswordWithMetadata(string password);

    /// <summary>
    /// Verifies a password against a metadata-formatted hash.
    /// </summary>
    /// <param name="password">The plain-text password to verify.</param>
    /// <param name="storedHashWithMetadata">The stored hash with metadata.</param>
    /// <returns>True if the password matches; otherwise, false.</returns>
    bool VerifyPasswordWithMetadata(string password, string storedHashWithMetadata);
}
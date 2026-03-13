using IotGrpcLearning.Interfaces;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace IotGrpcLearning.Infrastructure;

/// <summary>
/// Provides secure password hashing and verification using PBKDF2.
/// </summary>
public sealed class PasswordService : IPasswordService
{
    private readonly int _saltSize;
    private readonly int _keySize;
    private readonly int _iterations;

    public PasswordService(IOptions<PasswordOptions> options)
    {
        var config = options?.Value ?? new PasswordOptions();
        _saltSize = config.SaltSize;
        _keySize = config.KeySize;
        _iterations = config.Iterations;
    }

    public (string hash, string salt) HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or whitespace.", nameof(password));

        // 1. Generate random salt
        using var rng = RandomNumberGenerator.Create();
        byte[] saltBytes = new byte[_saltSize];
        rng.GetBytes(saltBytes);

        // 2. Derive key from password + salt
        using var pbkdf2 = new Rfc2898DeriveBytes(
            password,
            saltBytes,
            _iterations,
            HashAlgorithmName.SHA256);

        byte[] key = pbkdf2.GetBytes(_keySize);

        // 3. Convert to Base64 strings for storage
        string salt = Convert.ToBase64String(saltBytes);
        string hash = Convert.ToBase64String(key);

        return (hash, salt);
    }

    public bool VerifyPassword(string password, string storedHash, string storedSalt)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        if (string.IsNullOrWhiteSpace(storedHash) || string.IsNullOrWhiteSpace(storedSalt))
            return false;

        try
        {
            // 1. Decode salt
            byte[] saltBytes = Convert.FromBase64String(storedSalt);

            // 2. Derive key again from provided password + stored salt
            using var pbkdf2 = new Rfc2898DeriveBytes(
                password,
                saltBytes,
                _iterations,
                HashAlgorithmName.SHA256);

            byte[] key = pbkdf2.GetBytes(_keySize);
            byte[] storedHashBytes = Convert.FromBase64String(storedHash);

            // 3. Constant-time comparison
            return CryptographicOperations.FixedTimeEquals(key, storedHashBytes);
        }
        catch
        {
            return false;
        }
    }

    public string HashPasswordWithMetadata(string password)
    {
        var (hash, salt) = HashPassword(password);
        
        // Format: v1$iterations$salt$hash
        return $"v1${_iterations}${salt}${hash}";
    }

    public bool VerifyPasswordWithMetadata(string password, string storedHashWithMetadata)
    {
        if (string.IsNullOrWhiteSpace(storedHashWithMetadata))
            return false;

        var parts = storedHashWithMetadata.Split('$');
        
        // Check for legacy format (no metadata)
        if (parts.Length == 2)
        {
            // Assume it's "hash$salt" from old PasswordHasher
            return VerifyPassword(password, parts[0], parts[1]);
        }

        // New format: v1$iterations$salt$hash
        if (parts.Length != 4 || parts[0] != "v1")
            return false;

        if (!int.TryParse(parts[1], out int iterations))
            return false;

        string salt = parts[2];
        string hash = parts[3];

        return VerifyPassword(password, hash, salt);
    }
}
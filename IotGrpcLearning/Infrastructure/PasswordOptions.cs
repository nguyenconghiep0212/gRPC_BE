namespace IotGrpcLearning.Infrastructure;

/// <summary>
/// Configuration options for password hashing.
/// </summary>
public sealed class PasswordOptions
{
    /// <summary>
    /// Salt size in bytes. Default is 16 (128 bits).
    /// </summary>
    public int SaltSize { get; set; } = 16;

    /// <summary>
    /// Key size in bytes. Default is 32 (256 bits).
    /// </summary>
    public int KeySize { get; set; } = 32;

    /// <summary>
    /// Number of PBKDF2 iterations. Default is 100,000.
    /// Higher values increase security but also computation time.
    /// </summary>
    public int Iterations { get; set; } = 100_000;
}
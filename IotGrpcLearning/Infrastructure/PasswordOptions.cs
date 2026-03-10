namespace IotGrpcLearning.Infrastructure
{
    /// <summary>
    /// Password hashing options (kept small for Phase 0). Bind to config section "Password".
    /// </summary>
    public sealed class PasswordOptions
    {
        /// <summary>
        /// Number of PBKDF2 iterations. Make configurable to allow tuning without code changes.
        /// </summary>
        public int Iterations { get; set; } = 100_000;
    }
}
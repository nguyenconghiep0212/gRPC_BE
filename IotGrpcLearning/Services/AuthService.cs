using IotGrpcLearning.Interfaces;
using IotGrpcLearning.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace IotGrpcLearning.Services
{
    /// <summary>
    /// Service responsible for user authentication and JWT token management.
    /// </summary>
    public sealed class AuthService : IAuthService
    {
        private readonly IUser _userRepository;
        private readonly IPasswordService _passwordService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;
        private readonly TimeSpan _tokenLifetime;

        public AuthService(IUser userRepository, IPasswordService passwordService, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            var minutes = 60;
            if (int.TryParse(_configuration["Jwt:ExpiresMinutes"], out var m))
                minutes = m;

            _tokenLifetime = TimeSpan.FromMinutes(minutes);
        }

        public async Task<AuthResultDto> LoginAsync(LoginRequestDto request, CancellationToken ct = default)
        {
            if (request is null)
                return new AuthResultDto(false, "Invalid request payload");

            var user = await _userRepository.GetByUsernameAsync(request.Username, ct);
            if (user == null)
                return new AuthResultDto(false, "Invalid username or password");

            var verified = _passwordService.VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt);
            if (!verified)
                return new AuthResultDto(false, "Invalid username or password");

            var (token, expiresAt) = GenerateToken(user);

            var data = new LoginResponseDto(user.Id, user.Username, token, expiresAt);
            return new AuthResultDto(true, null, data);
        }               

        public async Task<AuthResultDto> RegisterAsync(RegisterRequestDto request, CancellationToken ct = default)
        {
            if (request is null)
                return new AuthResultDto(false, "Invalid request payload"); 

            var existing = await _userRepository.GetByUsernameAsync(request.Username, ct);
            if (existing != null)
                return new AuthResultDto(false, "Username already taken");

            var (hash, salt) = _passwordService.HashPassword(request.Password);

            var now = DateTime.UtcNow;
            var user = new UserDto(
                Id: 0,
                EmployeeId: request.EmployeeId,
                Username: request.Username.ToLowerInvariant(),
                PasswordHash: hash,
                PasswordSalt: salt,
                IsActive: true,
                LastLogin: now,
                CreatedAt: now,
                UpdatedAt: now
            );

            var created = await _userRepository.CreateAsync(user, ct);

            var (token, expiresAt) = GenerateToken(created);
            var data = new LoginResponseDto(created.Id, created.Username, token, expiresAt);

            return new AuthResultDto(true, null, data);
        }

        public async Task<UserDto?> ValidateTokenAsync(string token, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var secret = _configuration["Jwt:Secret"] ?? string.Empty;
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = handler.ValidateToken(token, validationParameters, out var validatedToken);
                var username = principal.Identity?.Name ?? principal.FindFirst(JwtRegisteredClaimNames.UniqueName)?.Value;
                if (string.IsNullOrWhiteSpace(username))
                    return null;

                return await _userRepository.GetByUsernameAsync(username, ct);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Token validation failed");
                return null;
            }
        }

        public async Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword, CancellationToken ct = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, ct);
            if (user == null)
                return false;

            // Verify old password
            if (!_passwordService.VerifyPassword(oldPassword, user.PasswordHash, user.PasswordSalt))
                return false;

            // Hash new password
            var (hash, salt) = _passwordService.HashPassword(newPassword);

            var updatedUser = user with
            {
                PasswordHash = hash,
                PasswordSalt = salt,
                UpdatedAt = DateTime.UtcNow
            };

            return await _userRepository.UpdateAsync(userId, updatedUser, ct);
        }

        private (string Token, DateTime ExpiresAt) GenerateToken(UserDto user)
        {
            var secret = _configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret is not configured");
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expires = DateTime.UtcNow.Add(_tokenLifetime);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username)
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return (tokenString, expires);
        }
    }
}

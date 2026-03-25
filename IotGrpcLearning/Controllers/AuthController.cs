using IotGrpcLearning.Interfaces;
using IotGrpcLearning.Models;
using Microsoft.AspNetCore.Mvc;

namespace IotGrpcLearning.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Authenticates a user with email and password.
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Authentication result with JWT token</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResultDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken ct)
    {
        if (request == null)
        {
            return BadRequest(new AuthResultDto(false, "Invalid request."));
        }

        var result = await _authService.LoginAsync(request, ct);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Registers a new user account.
    /// </summary>
    /// <param name="request">Registration details</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Authentication result with JWT token</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResultDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(AuthResultDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request, CancellationToken ct)
    {
        if (request == null)
        {
            return BadRequest(new AuthResultDto(false, "Invalid request."));
        }

        var result = await _authService.RegisterAsync(request, ct);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(Register), result);
    }

    /// <summary>
    /// Validates the current JWT token.
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>User information if token is valid</returns>
    [HttpGet("validate")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ValidateToken(CancellationToken ct)
    {
        var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        if (string.IsNullOrWhiteSpace(token))
        {
            return Unauthorized(new { message = "No token provided." });
        }

        var user = await _authService.ValidateTokenAsync(token, ct);

        if (user == null)
        {
            return Unauthorized(new { message = "Invalid or expired token." });
        }

        // Don't return password hash/salt
        var safeUser = new
        {
            user.Id,
            user.EmployeeId,
            user.Username,
            user.CreatedAt,
            user.IsActive
        };

        return Ok(safeUser);
    }

    /// <summary>
    /// Changes the password for the authenticated user.
    /// </summary>
    /// <param name="request">Old and new password</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpPost("change-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request, CancellationToken ct)
    {
        var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        if (string.IsNullOrWhiteSpace(token))
        {
            return Unauthorized(new { message = "No token provided." });
        }

        var user = await _authService.ValidateTokenAsync(token, ct);

        if (user == null)
        {
            return Unauthorized(new { message = "Invalid or expired token." });
        }

        var success = await _authService.ChangePasswordAsync(user.Id, request.OldPassword, request.NewPassword, ct);

        if (!success)
        {
            return BadRequest(new { message = "Failed to change password. Please check your old password." });
        }

        return Ok(new { message = "Password changed successfully." });
    }
}

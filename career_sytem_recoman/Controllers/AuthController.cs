using career_sytem_recoman.Models.DTOs.Auth;
using career_sytem_recoman.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace career_sytem_recoman.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(IAuthService authService) : ControllerBase
{
    private readonly IAuthService _authService = authService;

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var result = await _authService.RegisterAsync(dto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            var innerMessage = ex.InnerException?.Message ?? string.Empty;
            if (innerMessage.Contains("CHECK constraint") || ex.Message.Contains("UserType"))
            {
                return BadRequest(new { error = "Invalid UserType. Allowed values are 'Employee' or 'Company'." });
            }
            return StatusCode(500, new { error = "An unexpected error occurred.", details = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.LoginAsync(dto);
        return Ok(result);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.ForgotPasswordAsync(dto);
        return Ok(result);
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.ResetPasswordAsync(dto);
        return Ok(result);
    }

    [HttpPost("social-login")]
    public async Task<IActionResult> SocialLogin([FromBody] SocialLoginDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.SocialLoginAsync(dto);
        return Ok(result);
    }
}
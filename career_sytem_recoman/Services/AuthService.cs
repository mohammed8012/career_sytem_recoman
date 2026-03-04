using career_sytem_recoman.Models.DTOs.Auth;
using career_sytem_recoman.Models.Entities;
using career_sytem_recoman.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BCrypt.Net; // تأكد من وجود هذه

namespace career_sytem_recoman.Services;

public class AuthService(JobPlatformContext context, IConfiguration configuration) : IAuthService
{
    private readonly JobPlatformContext _context = context;
    private readonly IConfiguration _configuration = configuration;

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (existingUser is not null)
            throw new Exception("User already exists.");

        var user = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password), // استخدام الاسم المؤهل
            Phone = dto.Phone,
            UserType = dto.UserType,
            Location = dto.Location,
            Bio = dto.Bio,
            Skills = dto.Skills,
            YearsOfExperience = dto.YearsOfExperience,
            Specialization = dto.Specialization,
            CompanyName = dto.CompanyName,
            CompanyAddress = dto.CompanyAddress,
            FieldsAvailable = dto.FieldsAvailable,
            FoundedYear = dto.FoundedYear,
            CompanySize = dto.CompanySize,
            LogoPath = dto.LogoPath,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = GenerateJwtToken(user);
        return new AuthResponseDto
        {
            Token = token,
            UserId = user.UserId,
            Email = user.Email!,
            FirstName = user.FirstName ?? string.Empty,
            LastName = user.LastName ?? string.Empty,
            UserType = user.UserType ?? string.Empty
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new Exception("Invalid credentials.");

        var token = GenerateJwtToken(user);
        return new AuthResponseDto
        {
            Token = token,
            UserId = user.UserId,
            Email = user.Email!,
            FirstName = user.FirstName ?? string.Empty,
            LastName = user.LastName ?? string.Empty,
            UserType = user.UserType ?? string.Empty
        };
    }

    public async Task<AuthResponseDto> ForgotPasswordAsync(ForgotPasswordDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user is null)
            throw new Exception("User not found.");

        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))
                               .Replace("/", "_").Replace("+", "-");

        var resetToken = new PasswordResetToken
        {
            UserId = user.UserId,
            Token = token,
            ExpiryDate = DateTime.UtcNow.AddHours(1),
            IsUsed = false
        };
        _context.PasswordResetTokens.Add(resetToken);
        await _context.SaveChangesAsync();

        return new AuthResponseDto
        {
            Message = "Password reset token generated successfully.",
            Token = token
        };
    }

    public async Task<AuthResponseDto> ResetPasswordAsync(ResetPasswordDto dto)
    {
        var resetToken = await _context.PasswordResetTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == dto.Token && !rt.IsUsed && rt.ExpiryDate > DateTime.UtcNow);

        if (resetToken is null)
            throw new Exception("Invalid or expired token.");

        var user = resetToken.User;
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        resetToken.IsUsed = true;

        await _context.SaveChangesAsync();

        var jwtToken = GenerateJwtToken(user);
        return new AuthResponseDto
        {
            Message = "Password reset successfully.",
            Token = jwtToken,
            UserId = user.UserId,
            Email = user.Email!,
            FirstName = user.FirstName ?? string.Empty,
            LastName = user.LastName ?? string.Empty,
            UserType = user.UserType ?? string.Empty
        };
    }

    public Task<AuthResponseDto> SocialLoginAsync(SocialLoginDto dto)
    {
        throw new NotImplementedException("Social login not implemented.");
    }

    private string GenerateJwtToken(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var keyString = _configuration["Jwt:Key"];
        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];

        if (string.IsNullOrEmpty(keyString) || string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
            throw new InvalidOperationException("JWT settings are not configured.");

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Role, user.UserType ?? string.Empty)
        };

        if (!string.IsNullOrEmpty(user.FirstName))
            claims.Add(new(ClaimTypes.GivenName, user.FirstName));
        if (!string.IsNullOrEmpty(user.LastName))
            claims.Add(new(ClaimTypes.Surname, user.LastName));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.Now.AddDays(7),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
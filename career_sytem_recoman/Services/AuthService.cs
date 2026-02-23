using career_sytem_recoman.Models.DTOs.Auth;
using career_sytem_recoman.Models.Entities;
using career_sytem_recoman.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net; // ✅ استيراد BCrypt.Net

namespace career_sytem_recoman.Services
{
    public class AuthService : IAuthService
    {
        private readonly JobPlatformContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(JobPlatformContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            // التحقق من وجود المستخدم
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (existingUser != null)
                throw new Exception("User already exists.");

            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password), // ✅ استخدام الاسم المؤهل
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
                Email = user.Email,
                FirstName = user.FirstName ?? "",
                LastName = user.LastName ?? "",
                UserType = user.UserType
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash)) // ✅ استخدام الاسم المؤهل
                throw new Exception("Invalid credentials.");

            var token = GenerateJwtToken(user);
            return new AuthResponseDto
            {
                Token = token,
                UserId = user.UserId,
                Email = user.Email,
                FirstName = user.FirstName ?? "",
                LastName = user.LastName ?? "",
                UserType = user.UserType
            };
        }

        public Task<AuthResponseDto> ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            // يمكن تنفيذ هذه الدالة لاحقاً
            throw new NotImplementedException();
        }

        public Task<AuthResponseDto> ResetPasswordAsync(ResetPasswordDto dto)
        {
            throw new NotImplementedException();
        }

        public Task<AuthResponseDto> SocialLoginAsync(SocialLoginDto dto)
        {
            throw new NotImplementedException();
        }

        private string GenerateJwtToken(User user)
        {
            // التحقق من أن user ليس null (لإزالة تحذير null reference)
            if (user == null) throw new ArgumentNullException(nameof(user));

            var keyString = _configuration["Jwt:Key"];
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];

            if (string.IsNullOrEmpty(keyString) || string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
                throw new InvalidOperationException("JWT settings are not configured.");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Role, user.UserType ?? "")
            };

            // إضافة الأسماء فقط إذا كانت موجودة
            if (!string.IsNullOrEmpty(user.FirstName))
                claims.Add(new Claim(ClaimTypes.GivenName, user.FirstName));
            if (!string.IsNullOrEmpty(user.LastName))
                claims.Add(new Claim(ClaimTypes.Surname, user.LastName));

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
}
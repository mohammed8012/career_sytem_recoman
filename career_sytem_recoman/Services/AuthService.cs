using career_sytem_recoman.Models.DTOs.Auth;
using career_sytem_recoman.Models.DTOs.User;
using career_sytem_recoman.Models.Entities;
using career_sytem_recoman.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BCrypt.Net;
using Newtonsoft.Json.Linq;

namespace career_sytem_recoman.Services
{
    public class AuthService : IAuthService
    {
        private readonly JobPlatformContext _context;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly IUserService _userService;
        private readonly IAiCvService _aiCvService;

        public AuthService(JobPlatformContext context, IConfiguration configuration, HttpClient httpClient, IUserService userService, IAiCvService aiCvService)
        {
            _context = context;
            _configuration = configuration;
            _httpClient = httpClient;
            _userService = userService;
            _aiCvService = aiCvService;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            // التحقق من وجود المستخدم
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (existingUser != null)
                throw new Exception("User already exists.");

            // إنشاء المستخدم (مع حفظ JobDescription إذا وجد)
            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
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
                CreatedAt = DateTime.UtcNow,
                JobDescription = dto.JobDescription   // ✅ حفظ الوصف الوظيفي
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // ❌ تمت إزالة كود رفع وتحليل CV بالكامل

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

        // باقي الدوال (LoginAsync, ForgotPasswordAsync, ResetPasswordAsync, SocialLoginAsync, إلخ) تبقى كما هي
        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
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
            if (user == null)
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

            if (resetToken == null)
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

        public async Task<AuthResponseDto> SocialLoginAsync(SocialLoginDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Provider) || string.IsNullOrEmpty(dto.Token))
                throw new Exception("Invalid social login data.");

            string provider = dto.Provider.ToLower();
            string providerId = "";
            string email = "";
            string firstName = "";
            string lastName = "";

            try
            {
                if (provider == "google")
                {
                    var payload = await VerifyGoogleToken(dto.Token);
                    if (payload == null)
                        throw new Exception("Invalid Google token.");

                    providerId = payload.Subject;
                    email = payload.Email;
                    firstName = payload.GivenName;
                    lastName = payload.FamilyName;
                }
                else if (provider == "facebook")
                {
                    var fbInfo = await VerifyFacebookToken(dto.Token);
                    if (fbInfo == null)
                        throw new Exception("Invalid Facebook token.");

                    providerId = fbInfo.Id;
                    email = fbInfo.Email;
                    firstName = fbInfo.FirstName;
                    lastName = fbInfo.LastName;
                }
                else
                {
                    throw new Exception($"Unsupported provider: {provider}");
                }

                if (string.IsNullOrEmpty(email))
                    throw new Exception("Email not provided by the provider.");

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Provider == provider && u.ProviderId == providerId);

                if (user == null)
                    user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

                if (user == null)
                {
                    user = new User
                    {
                        Email = email,
                        FirstName = firstName,
                        LastName = lastName,
                        Provider = provider,
                        ProviderId = providerId,
                        UserType = "Employee",
                        CreatedAt = DateTime.UtcNow,
                        PasswordHash = ""
                    };
                    _context.Users.Add(user);
                }
                else
                {
                    if (string.IsNullOrEmpty(user.Provider))
                    {
                        user.Provider = provider;
                        user.ProviderId = providerId;
                    }
                }

                await _context.SaveChangesAsync();

                var token = GenerateJwtToken(user);
                return new AuthResponseDto
                {
                    Token = token,
                    UserId = user.UserId,
                    Email = user.Email!,
                    FirstName = user.FirstName ?? string.Empty,
                    LastName = user.LastName ?? string.Empty,
                    UserType = user.UserType ?? string.Empty,
                    Message = "Social login successful."
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Social login failed: {ex.Message}");
            }
        }

        private async Task<GooglePayload?> VerifyGoogleToken(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var json = await _httpClient.GetStringAsync("https://www.googleapis.com/oauth2/v3/certs");
                var keys = new JsonWebKeySet(json);
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuers = new[] { "accounts.google.com", "https://accounts.google.com" },
                    ValidateAudience = true,
                    ValidAudience = _configuration["Google:ClientId"],
                    ValidateLifetime = true,
                    IssuerSigningKeys = keys.GetSigningKeys(),
                    ClockSkew = TimeSpan.FromMinutes(5)
                };

                var principal = handler.ValidateToken(token, validationParameters, out var validatedToken);
                var jwtToken = validatedToken as JwtSecurityToken;
                if (jwtToken == null)
                    return null;

                return new GooglePayload
                {
                    Subject = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value ?? "",
                    Email = jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value ?? "",
                    GivenName = jwtToken.Claims.FirstOrDefault(c => c.Type == "given_name")?.Value ?? "",
                    FamilyName = jwtToken.Claims.FirstOrDefault(c => c.Type == "family_name")?.Value ?? ""
                };
            }
            catch
            {
                return null;
            }
        }

        private async Task<FacebookPayload?> VerifyFacebookToken(string token)
        {
            try
            {
                var appId = _configuration["Facebook:AppId"];
                var appSecret = _configuration["Facebook:AppSecret"];
                var url = $"https://graph.facebook.com/me?access_token={token}&fields=id,name,email,first_name,last_name";
                var response = await _httpClient.GetStringAsync(url);
                var data = JObject.Parse(response);

                var debugUrl = $"https://graph.facebook.com/debug_token?input_token={token}&access_token={appId}|{appSecret}";
                var debugResponse = await _httpClient.GetStringAsync(debugUrl);
                var debugData = JObject.Parse(debugResponse);
                if (debugData["data"]?["is_valid"]?.Value<bool>() != true)
                    return null;

                return new FacebookPayload
                {
                    Id = data["id"]?.ToString() ?? "",
                    Email = data["email"]?.ToString() ?? "",
                    FirstName = data["first_name"]?.ToString() ?? "",
                    LastName = data["last_name"]?.ToString() ?? ""
                };
            }
            catch
            {
                return null;
            }
        }

        private string GenerateJwtToken(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var keyString = _configuration["Jwt:Key"];
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];

            if (string.IsNullOrEmpty(keyString) || string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
                throw new InvalidOperationException("JWT settings are not configured.");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Role, user.UserType ?? string.Empty)
            };

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

        private class GooglePayload
        {
            public string? Subject { get; set; }
            public string? Email { get; set; }
            public string? GivenName { get; set; }
            public string? FamilyName { get; set; }
        }

        private class FacebookPayload
        {
            public string? Id { get; set; }
            public string? Email { get; set; }
            public string? FirstName { get; set; }
            public string? LastName { get; set; }
        }
    }
}
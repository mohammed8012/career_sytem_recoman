using career_sytem_recoman.Models.DTOs.Auth;

namespace career_sytem_recoman.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task<AuthResponseDto> ForgotPasswordAsync(ForgotPasswordDto dto);
        Task<AuthResponseDto> ResetPasswordAsync(ResetPasswordDto dto);
        Task<AuthResponseDto> SocialLoginAsync(SocialLoginDto dto);
    }
}
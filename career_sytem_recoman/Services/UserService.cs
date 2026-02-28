#nullable enable
using career_sytem_recoman.Models.DTOs.Application;
using career_sytem_recoman.Models.DTOs.Course;
using career_sytem_recoman.Models.DTOs.User;
using career_sytem_recoman.Models.Entities;
using career_sytem_recoman.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace career_sytem_recoman.Services;

public class UserService(JobPlatformContext context, IWebHostEnvironment env) : IUserService
{
    public async Task<UserProfileDto> GetProfileAsync(int userId)
    {
        var user = await context.Users
            .Include(u => u.Applications)
                .ThenInclude(a => a.Job)
            .Include(u => u.CourseTrackings)
                .ThenInclude(ct => ct.Course)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user is null)
            throw new Exception("User not found.");

        return new UserProfileDto
        {
            UserId = user.UserId,
            UserType = user.UserType,
            Email = user.Email,
            Phone = user.Phone,
            Location = user.Location,
            CreatedAt = user.CreatedAt,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Cvpath = user.Cvpath,
            Bio = user.Bio,
            Skills = user.Skills,
            YearsOfExperience = user.YearsOfExperience,
            CompanyName = user.CompanyName,
            CompanyAddress = user.CompanyAddress,
            Specialization = user.Specialization,
            FieldsAvailable = user.FieldsAvailable,
            FoundedYear = user.FoundedYear,
            CompanySize = user.CompanySize,
            LogoPath = user.LogoPath,
            Applications = user.Applications?.Select(a => new ApplicationDto
            {
                ApplicationId = a.ApplicationId,
                UserId = a.UserId,
                JobId = a.JobId,
                InteractionType = a.InteractionType,
                Status = a.Status,
                AppliedAt = a.AppliedAt,
                CompanyNotes = a.CompanyNotes
            }).ToList() ?? [],
            CourseTrackings = user.CourseTrackings?.Select(ct => new CourseTrackingDto
            {
                TrackId = ct.TrackId,
                UserId = ct.UserId,
                CourseId = ct.CourseId,
                ProgressPercent = ct.ProgressPercent,
                IsCompleted = ct.IsCompleted,
                LastAccessed = ct.LastAccessed,
                Rating = ct.Rating,
                Review = ct.Review
            }).ToList() ?? []
        };
    }

    public async Task<UserProfileDto> UpdateProfileAsync(int userId, UpdateProfileDto dto)
    {
        var user = await context.Users.FindAsync(userId);
        if (user is null)
            throw new Exception("User not found.");

        if (!string.IsNullOrEmpty(dto.FirstName))
            user.FirstName = dto.FirstName;
        if (!string.IsNullOrEmpty(dto.LastName))
            user.LastName = dto.LastName;
        if (!string.IsNullOrEmpty(dto.Phone))
            user.Phone = dto.Phone;
        if (!string.IsNullOrEmpty(dto.Location))
            user.Location = dto.Location;
        if (!string.IsNullOrEmpty(dto.Bio))
            user.Bio = dto.Bio;
        if (!string.IsNullOrEmpty(dto.Skills))
            user.Skills = dto.Skills;
        if (dto.YearsOfExperience.HasValue)
            user.YearsOfExperience = dto.YearsOfExperience;
        if (!string.IsNullOrEmpty(dto.CompanyName))
            user.CompanyName = dto.CompanyName;
        if (!string.IsNullOrEmpty(dto.CompanyAddress))
            user.CompanyAddress = dto.CompanyAddress;
        if (!string.IsNullOrEmpty(dto.Specialization))
            user.Specialization = dto.Specialization;
        if (!string.IsNullOrEmpty(dto.FieldsAvailable))
            user.FieldsAvailable = dto.FieldsAvailable;
        if (dto.FoundedYear.HasValue)
            user.FoundedYear = dto.FoundedYear;
        if (!string.IsNullOrEmpty(dto.CompanySize))
            user.CompanySize = dto.CompanySize;
        if (!string.IsNullOrEmpty(dto.LogoPath))
            user.LogoPath = dto.LogoPath;

        await context.SaveChangesAsync();
        return await GetProfileAsync(userId);
    }

    public async Task<string> UploadCvAsync(int userId, IFormFile file)
    {
        if (file is null || file.Length == 0)
            throw new Exception("No file uploaded.");

        var allowedExtensions = new[] { ".pdf", ".docx" };
        var ext = Path.GetExtension(file.FileName);
        if (!allowedExtensions.Any(e => string.Equals(e, ext, StringComparison.OrdinalIgnoreCase)))
            throw new Exception("Only PDF and DOCX files are allowed.");

        // التأكد من وجود مسار تخزين صالح
        string webRootPath = env.WebRootPath;
        if (string.IsNullOrEmpty(webRootPath))
        {
            webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        }

        var uploadsFolder = Path.Combine(webRootPath, "uploads", "cvs");
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var fileName = $"{userId}_{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var user = await context.Users.FindAsync(userId);
        if (user is null)
            throw new Exception("User not found.");

        // حذف السيرة الذاتية القديمة إذا وجدت
        if (!string.IsNullOrEmpty(user.Cvpath))
        {
            var oldPath = Path.Combine(webRootPath, user.Cvpath.TrimStart('/'));
            if (File.Exists(oldPath))
                File.Delete(oldPath);
        }

        user.Cvpath = $"/uploads/cvs/{fileName}";
        await context.SaveChangesAsync();

        return user.Cvpath;
    }

    public async Task<(Stream Stream, string ContentType, string FileName)> GetCvFileAsync(int userId)
    {
        string webRootPath = env.WebRootPath;
        if (string.IsNullOrEmpty(webRootPath))
        {
            webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        }

        var user = await context.Users.FindAsync(userId);
        if (user is null || string.IsNullOrEmpty(user.Cvpath))
            throw new FileNotFoundException("CV not found.");

        var filePath = Path.Combine(webRootPath, user.Cvpath.TrimStart('/'));
        if (!File.Exists(filePath))
            throw new FileNotFoundException("CV file does not exist on server.");

        var stream = File.OpenRead(filePath);
        var contentType = string.Equals(Path.GetExtension(filePath), ".pdf", StringComparison.OrdinalIgnoreCase)
            ? "application/pdf"
            : "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
        var fileName = Path.GetFileName(filePath);

        return (stream, contentType, fileName);
    }
}
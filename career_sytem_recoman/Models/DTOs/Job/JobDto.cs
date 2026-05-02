using System;
using System.Collections.Generic;
using career_sytem_recoman.Models.DTOs.Application;
using career_sytem_recoman.Models.DTOs.User;

namespace career_sytem_recoman.Models.DTOs.Job
{
    public class JobDto
    {
        public int JobId { get; set; }
        public int CompanyId { get; set; }
        public string JobTitle { get; set; } = null!;
        public string? JobCategory { get; set; }
        public string? Description { get; set; }
        public string? Requirements { get; set; }
        public string? Location { get; set; }
        public string? JobType { get; set; }
        public int? MinExperience { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateOnly? ExpiryDate { get; set; }
        public bool? IsActive { get; set; }

        // معلومات الشركة (صاحب العمل)
        public UserProfileDto? Company { get; set; }

        // 👇 أضف هذه الخاصية الجديدة
        public string? CompanyName { get; set; }

        // قائمة المتقدمين (للاستخدام من قبل الشركة)
        public List<ApplicationDto>? Applications { get; set; }

        // عدد المتقدمين (محسوب)
        public int ApplicantsCount => Applications?.Count ?? 0;

        // درجة المطابقة (للتوصيات)
        public double MatchScore { get; set; } = 0;

        // ✅ خاصية محسوبة: هل انتهت صلاحية الوظيفة؟
        public bool IsExpired => ExpiryDate.HasValue && ExpiryDate.Value <= DateOnly.FromDateTime(DateTime.Now);
    }
}
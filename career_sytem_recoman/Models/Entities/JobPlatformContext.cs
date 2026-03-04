using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace career_sytem_recoman.Models.Entities;

public partial class JobPlatformContext : DbContext
{
    public JobPlatformContext()
    {
    }

    public JobPlatformContext(DbContextOptions<JobPlatformContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Application> Applications { get; set; }

    public virtual DbSet<Communication> Communications { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<CourseTracking> CourseTrackings { get; set; }

    public virtual DbSet<Job> Jobs { get; set; }

    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<Post> Posts { get; set; }
    public virtual DbSet<Rating> Ratings { get; set; }
    // 👇 تم إضافة هذا السطر (جدول رموز إعادة تعيين كلمة المرور)
    public virtual DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Application>(entity =>
        {
            entity.HasKey(e => e.ApplicationId).HasName("PK__Applicat__C93A4F7954EF847A");

            entity.HasIndex(e => e.JobId, "IX_Applications_JobID");

            entity.HasIndex(e => e.UserId, "IX_Applications_UserID");

            entity.Property(e => e.ApplicationId).HasColumnName("ApplicationID");
            entity.Property(e => e.AppliedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.InteractionType)
                .HasMaxLength(20)
                .HasDefaultValue("Applied");
            entity.Property(e => e.JobId).HasColumnName("JobID");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Pending");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Job).WithMany(p => p.Applications)
                .HasForeignKey(d => d.JobId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Applications_Job");

            entity.HasOne(d => d.User).WithMany(p => p.Applications)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Applications_User");
        });

        modelBuilder.Entity<Communication>(entity =>
        {
            entity.HasKey(e => e.CommId).HasName("PK__Communic__AF8CE2B9AC2710F9");

            entity.HasIndex(e => new { e.ReceiverType, e.ReceiverId }, "IX_Communications_Receiver");

            entity.Property(e => e.CommId).HasColumnName("CommID");
            entity.Property(e => e.CommType).HasMaxLength(20);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsRead).HasDefaultValue(false);
            entity.Property(e => e.NotificationType).HasMaxLength(20);
            entity.Property(e => e.ReceiverId).HasColumnName("ReceiverID");
            entity.Property(e => e.ReceiverType).HasMaxLength(20);
            entity.Property(e => e.SenderId).HasColumnName("SenderID");
            entity.Property(e => e.SenderType).HasMaxLength(20);
            entity.Property(e => e.Title).HasMaxLength(255);
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.CourseId).HasName("PK__Courses__C92D7187E7A6870E");

            entity.HasIndex(e => e.Category, "IX_Courses_Category");

            entity.Property(e => e.CourseId).HasColumnName("CourseID");
            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.CourseUrl)
                .HasMaxLength(255)
                .HasColumnName("CourseURL");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(255)
                .HasColumnName("ImageURL");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Provider).HasMaxLength(100);
            entity.Property(e => e.Title).HasMaxLength(200);
        });

        modelBuilder.Entity<CourseTracking>(entity =>
        {
            entity.HasKey(e => e.TrackId).HasName("PK__CourseTr__7A74F8C0507ACDA3");

            entity.ToTable("CourseTracking");

            entity.Property(e => e.TrackId).HasColumnName("TrackID");
            entity.Property(e => e.CourseId).HasColumnName("CourseID");
            entity.Property(e => e.IsCompleted).HasDefaultValue(false);
            entity.Property(e => e.LastAccessed).HasColumnType("datetime");
            entity.Property(e => e.ProgressPercent).HasDefaultValue(0);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Course).WithMany(p => p.CourseTrackings)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tracking_Course");

            entity.HasOne(d => d.User).WithMany(p => p.CourseTrackings)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tracking_User");
        });

        modelBuilder.Entity<Job>(entity =>
        {
            entity.HasKey(e => e.JobId).HasName("PK__Jobs__056690E2DF5BC31B");

            entity.HasIndex(e => e.JobCategory, "IX_Jobs_Category");

            entity.HasIndex(e => e.CompanyId, "IX_Jobs_CompanyID");

            entity.Property(e => e.JobId).HasColumnName("JobID");
            entity.Property(e => e.CompanyId).HasColumnName("CompanyID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.JobCategory).HasMaxLength(50);
            entity.Property(e => e.JobTitle).HasMaxLength(100);
            entity.Property(e => e.JobType)
                .HasMaxLength(20)
                .HasDefaultValue("Full-time");
            entity.Property(e => e.Location).HasMaxLength(100);
            entity.Property(e => e.MinExperience).HasDefaultValue(0);

            entity.HasOne(d => d.Company).WithMany(p => p.Jobs)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Jobs_Company");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CCACAFAD5364");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D1053456DCE8FC").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.CompanyName).HasMaxLength(100);
            entity.Property(e => e.CompanySize)
                .HasMaxLength(20)
                .HasDefaultValue("Small");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Cvpath)
                .HasMaxLength(255)
                .HasColumnName("CVPath");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.Location).HasMaxLength(100);
            entity.Property(e => e.LogoPath).HasMaxLength(255);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Specialization).HasMaxLength(100);
            entity.Property(e => e.UserType).HasMaxLength(20);
            entity.Property(e => e.YearsOfExperience).HasDefaultValue(0);
        });

        // 👇 تم إضافة تكوين جدول PasswordResetToken هنا
        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Token).IsUnique();
            entity.Property(e => e.Token).HasMaxLength(255);
            entity.Property(e => e.ExpiryDate).HasColumnType("datetime");
            entity.Property(e => e.IsUsed).HasDefaultValue(false);

            entity.HasOne(e => e.User)
                  .WithMany() // إذا لم يكن للمستخدم خاصية تجميعية، استخدم WithMany() فارغة
                  .HasForeignKey(e => e.UserId)
                  .HasConstraintName("FK_PasswordResetTokens_Users")
                  .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.PostId);
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");

            entity.HasOne(e => e.Company)
                  .WithMany() // إذا أردت إضافة مجموعة Posts لـ User لاحقاً
                  .HasForeignKey(e => e.CompanyId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("FK_Posts_Company");
        });
        modelBuilder.Entity<Rating>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Value).IsRequired();
            entity.Property(e => e.Review).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");

            entity.HasOne(e => e.RatedByUser)
                  .WithMany()
                  .HasForeignKey(e => e.RatedByUserId)
                  .OnDelete(DeleteBehavior.ClientSetNull)
                  .HasConstraintName("FK_Ratings_RatedByUser");

            entity.HasOne(e => e.RatedUser)
                  .WithMany()
                  .HasForeignKey(e => e.RatedUserId)
                  .OnDelete(DeleteBehavior.ClientSetNull)
                  .HasConstraintName("FK_Ratings_RatedUser");

            entity.HasIndex(e => new { e.RatedByUserId, e.RatedUserId }).IsUnique();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
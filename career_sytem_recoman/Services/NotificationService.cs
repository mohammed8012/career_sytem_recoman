using career_sytem_recoman.Models.DTOs.Notification;
using career_sytem_recoman.Models.Entities;
using career_sytem_recoman.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace career_sytem_recoman.Services;

public class NotificationService(JobPlatformContext context) : INotificationService
{
    public async Task<List<NotificationDto>> GetNotificationsAsync(int userId)
    {
        var notifications = await context.Communications
            .Where(c => c.ReceiverId == userId && c.ReceiverType == "User")
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new NotificationDto
            {
                Id = c.CommId,
                Type = c.NotificationType ?? c.CommType ?? "Unknown",
                Content = c.Content,
                IsRead = c.IsRead ?? false,
                CreatedAt = c.CreatedAt ?? DateTime.UtcNow
            })
            .ToListAsync();

        return notifications;
    }

    public async Task MarkAsReadAsync(int notificationId, int userId)
    {
        var notification = await context.Communications
            .FirstOrDefaultAsync(c => c.CommId == notificationId && c.ReceiverId == userId);
        if (notification is not null)
        {
            notification.IsRead = true;
            await context.SaveChangesAsync();
        }
    }

    public async Task DeleteNotificationAsync(int notificationId, int userId)
    {
        var notification = await context.Communications
            .FirstOrDefaultAsync(c => c.CommId == notificationId && c.ReceiverId == userId);
        if (notification is not null)
        {
            context.Communications.Remove(notification);
            await context.SaveChangesAsync();
        }
    }
}
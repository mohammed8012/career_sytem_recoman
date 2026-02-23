using career_sytem_recoman.Models.DTOs.Notification;
using career_sytem_recoman.Models.Entities;
using career_sytem_recoman.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace career_sytem_recoman.Services
{
    public class NotificationService : INotificationService
    {
        private readonly JobPlatformContext _context;

        public NotificationService(JobPlatformContext context)
        {
            _context = context;
        }

        public async Task<List<NotificationDto>> GetNotificationsAsync(int userId)
        {
            // Assuming there is a Communications table for notifications
            var notifications = await _context.Communications
                .Where(c => c.ReceiverId == userId && c.ReceiverType == "User")
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new NotificationDto
                {
                    Id = c.CommId,
                    Type = c.NotificationType ?? c.CommType,
                    Content = c.Content,
                    IsRead = c.IsRead ?? false,
                    CreatedAt = c.CreatedAt ?? DateTime.UtcNow
                })
                .ToListAsync();

            return notifications;
        }

        public async Task MarkAsReadAsync(int notificationId, int userId)
        {
            var notification = await _context.Communications
                .FirstOrDefaultAsync(c => c.CommId == notificationId && c.ReceiverId == userId);
            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteNotificationAsync(int notificationId, int userId)
        {
            var notification = await _context.Communications
                .FirstOrDefaultAsync(c => c.CommId == notificationId && c.ReceiverId == userId);
            if (notification != null)
            {
                _context.Communications.Remove(notification);
                await _context.SaveChangesAsync();
            }
        }
    }
}
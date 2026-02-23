using career_sytem_recoman.Models.DTOs.Notification;

namespace career_sytem_recoman.Services.Interfaces
{
    public interface INotificationService
    {
        Task<List<NotificationDto>> GetNotificationsAsync(int userId);
        Task MarkAsReadAsync(int notificationId, int userId);
        Task DeleteNotificationAsync(int notificationId, int userId);
    }
}
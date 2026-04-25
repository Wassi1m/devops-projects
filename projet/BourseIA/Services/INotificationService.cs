using BourseIA.DTOs;

namespace BourseIA.Services;

public interface INotificationService
{
    Task<List<NotificationDto>> GetNotificationsAsync(int userId);
    Task MarquerLueAsync(int notificationId, int userId);
    Task MarquerToutesLuesAsync(int userId);
    Task CreerNotificationAsync(int userId, string message, string type = "Info", string? lienAction = null);
}

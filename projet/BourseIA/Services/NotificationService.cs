using BourseIA.Data;
using BourseIA.DTOs;
using BourseIA.Models;
using Microsoft.EntityFrameworkCore;

namespace BourseIA.Services;

public class NotificationService : INotificationService
{
    private readonly AppDbContext _db;

    public NotificationService(AppDbContext db) => _db = db;

    public async Task<List<NotificationDto>> GetNotificationsAsync(int userId)
    {
        return await _db.Notifications
            .Where(n => n.UtilisateurId == userId)
            .OrderByDescending(n => n.DateCreation)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Message = n.Message,
                Type = n.Type,
                EstLue = n.EstLue,
                DateCreation = n.DateCreation,
                LienAction = n.LienAction
            })
            .ToListAsync();
    }

    public async Task MarquerLueAsync(int notificationId, int userId)
    {
        var n = await _db.Notifications.FirstOrDefaultAsync(n =>
            n.Id == notificationId && n.UtilisateurId == userId);
        if (n is null) return;

        n.EstLue = true;
        await _db.SaveChangesAsync();
    }

    public async Task MarquerToutesLuesAsync(int userId)
    {
        var notifications = await _db.Notifications
            .Where(n => n.UtilisateurId == userId && !n.EstLue)
            .ToListAsync();

        notifications.ForEach(n => n.EstLue = true);
        await _db.SaveChangesAsync();
    }

    public async Task CreerNotificationAsync(int userId, string message, string type = "Info", string? lienAction = null)
    {
        _db.Notifications.Add(new Notification
        {
            UtilisateurId = userId,
            Message = message,
            Type = type,
            LienAction = lienAction
        });
        await _db.SaveChangesAsync();
    }
}

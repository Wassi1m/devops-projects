namespace BourseIA.DTOs;

public class NotificationDto
{
    public int Id { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool EstLue { get; set; }
    public DateTime DateCreation { get; set; }
    public string? LienAction { get; set; }
}

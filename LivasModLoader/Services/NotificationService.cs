using System.Collections.ObjectModel;
using LivasModLoader.Models;

namespace LivasModLoader.Services;

public class NotificationService
{
    public ObservableCollection<NotificationMessage> Notifications { get; } = new();

    public void Push(string title, string message)
    {
        Notifications.Insert(0, new NotificationMessage
        {
            Title = title,
            Message = message
        });
    }
}

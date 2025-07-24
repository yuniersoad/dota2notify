using System;

namespace Dota2Notify.Api.notifications;

public interface INotifyService
{
    Task SendNotificationAsync(string message, string chatId);
}

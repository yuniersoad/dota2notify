using System.Text;
using System.Text.Json;

namespace Dota2Notify.Api.notifications;

public class TelegramNotifyService : INotifyService
{
    private readonly HttpClient _httpClient;
    private readonly string _botToken;
   
    public TelegramNotifyService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _botToken = configuration.GetValueWithEnvOverride("Telegram:BotToken") ?? throw new ArgumentException("Telegram:BotToken is required");
    }

    public async Task SendNotificationAsync(string message, string chatId)
    {
        var url = $"https://api.telegram.org/bot{_botToken}/sendMessage";
        
        var payload = new
        {
            chat_id = chatId,
            text = message
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(url, content);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Failed to send Telegram message: {response.StatusCode} - {errorContent}");
        }
    }
}

using System.Text.Json.Serialization;

namespace Dota2Notify.Api.Models;

public class User
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("userId")]
    public long UserId { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("telegramChatId")]
    public string TelegramChatId { get; set; } = string.Empty;
    
    [JsonPropertyName("following")]
    public List<FollowedPlayer> Following { get; set; } = new List<FollowedPlayer>();
    
    [JsonPropertyName("type")]
    public string Type { get; set; } = "user";
}

using Newtonsoft.Json;

namespace Dota2Notify.Api.Models;

public class User
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("userId")]
    public long UserId { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("telegramChatId")]
    public string TelegramChatId { get; set; } = string.Empty;

    [JsonProperty("following")]
    public List<FollowedPlayer> Following { get; set; } = new();

    [JsonProperty("type")]
    public string Type { get; set; } = "user";
}

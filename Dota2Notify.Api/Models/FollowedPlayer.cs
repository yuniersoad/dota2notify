using System.Text.Json.Serialization;

namespace Dota2Notify.Api.Models;

public class FollowedPlayer
{
    [JsonPropertyName("userId")]
    public long UserId { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("lastMatchId")]
    public string LastMatchId { get; set; } = string.Empty;
}

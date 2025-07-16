using Newtonsoft.Json;

namespace Dota2Notify.Api.Models;

public class FollowedPlayer
{
    [JsonProperty("userId")]
    public long UserId { get; set; }
    
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonProperty("lastMatchId")]
    public long LastMatchId { get; set; }
}

using System.Text.Json.Serialization;

namespace Dota2Notify.Api.Models;

public class DotaMatch
{
    [JsonPropertyName("match_id")]
    public long MatchId { get; set; }

    [JsonPropertyName("player_slot")]
    public int PlayerSlot { get; set; }
    
    [JsonPropertyName("radiant_win")]
    public bool RadiantWin { get; set; }
    
    [JsonPropertyName("duration")]
    public int Duration { get; set; }
    
    [JsonPropertyName("game_mode")]
    public int GameMode { get; set; }
    
    [JsonPropertyName("lobby_type")]
    public int LobbyType { get; set; }
    
    [JsonPropertyName("hero_id")]
    public int HeroId { get; set; }
    
    [JsonPropertyName("start_time")]
    public long StartTime { get; set; }
    
    [JsonPropertyName("kills")]
    public int Kills { get; set; }
    
    [JsonPropertyName("deaths")]
    public int Deaths { get; set; }
    
    [JsonPropertyName("assists")]
    public int Assists { get; set; }

    // Additional properties
    [JsonIgnore]
    public bool PlayerWon => IsRadiant ? RadiantWin : !RadiantWin;
    
    [JsonIgnore]
    public bool IsRadiant => PlayerSlot < 128;
    
    [JsonIgnore]
    public DateTime StartTimeUtc => DateTimeOffset.FromUnixTimeSeconds(StartTime).UtcDateTime;
    
    [JsonIgnore]
    public TimeSpan MatchDuration => TimeSpan.FromSeconds(Duration);
    
    public override string ToString()
    {
        string result = PlayerWon ? "Won" : "Lost";
        return $"Match {MatchId}: {result} as Hero {HeroId} with KDA {Kills}/{Deaths}/{Assists} - {StartTimeUtc:g}";
    }
}

using System.Text.Json;
using Dota2Notify.Api.Models;

namespace Dota2Notify.Api.Services;

public class OpenDotaService : IDotaService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenDotaService> _logger;
    private readonly string _baseUrl = "https://api.opendota.com/api";
    
    public OpenDotaService(HttpClient httpClient, ILogger<OpenDotaService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }
    
    public async Task<IEnumerable<DotaMatch>> GetPlayerRecentMatchesAsync(long playerId, int limit = 10)
    {
        try
        {
            _logger.LogInformation($"Fetching {limit} recent matches for player {playerId}");
            
            var url = $"{_baseUrl}/players/{playerId}/matches?limit={limit}";
            var response = await _httpClient.GetAsync(url);
            
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            var matches = JsonSerializer.Deserialize<IEnumerable<DotaMatch>>(content, 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
            _logger.LogInformation($"Successfully retrieved {matches?.Count() ?? 0} matches for player {playerId}");
            
            return matches ?? Enumerable.Empty<DotaMatch>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, $"Error fetching matches for player {playerId}: {ex.Message}");
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, $"Error deserializing matches for player {playerId}: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error fetching matches for player {playerId}: {ex.Message}");
            throw;
        }
    }
}

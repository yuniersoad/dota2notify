using Dota2Notify.Api.Models;

namespace Dota2Notify.Api.Services;

public interface IDotaService
{
    /// <summary>
    /// Gets the most recent matches for a player
    /// </summary>
    /// <param name="playerId">The Dota 2 player ID</param>
    /// <param name="limit">Maximum number of matches to retrieve</param>
    /// <returns>List of recent matches</returns>
    Task<IEnumerable<DotaMatch>> GetPlayerRecentMatchesAsync(long playerId, int limit = 10);
}

using Dota2Notify.Api.Models;

namespace Dota2Notify.Api.Services;

public interface IUserService
{
    /// <summary>
    /// Gets a user by their ID
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>The user object or null if not found</returns>
    Task<User?> GetUserAsync(long userId);
    
    /// <summary>
    /// Gets all users
    /// </summary>
    /// <returns>List of all users</returns>
    Task<IEnumerable<User>> GetAllUsersAsync();
    
    /// <summary>
    /// Creates or updates a user
    /// </summary>
    /// <param name="user">The user to create or update</param>
    /// <returns>The created or updated user</returns>
    Task<User> UpsertUserAsync(User user);
    
    /// <summary>
    /// Adds a followed player to a user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="followedPlayer">The player to follow</param>
    /// <returns>The updated user</returns>
    Task<User?> AddFollowedPlayerAsync(long userId, FollowedPlayer followedPlayer);
    
    /// <summary>
    /// Updates the last match ID for a followed player
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="followedPlayerId">The ID of the followed player</param>
    /// <param name="lastMatchId">The new last match ID</param>
    /// <returns>True if updated successfully, false otherwise</returns>
    Task<bool> UpdateLastMatchIdAsync(long userId, long followedPlayerId, long lastMatchId);
}

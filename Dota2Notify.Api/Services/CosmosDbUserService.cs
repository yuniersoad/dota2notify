using System.Net;
using Dota2Notify.Api.Models;
using Microsoft.Azure.Cosmos;
using User = Dota2Notify.Api.Models.User;

namespace Dota2Notify.Api.Services;

public class CosmosDbUserService : IUserService
{
    private readonly CosmosClient _cosmosClient;
    private readonly Container _container;
    private readonly ILogger<CosmosDbUserService> _logger;
    
    public CosmosDbUserService(CosmosClient cosmosClient, IConfiguration configuration, ILogger<CosmosDbUserService> logger)
    {
        _cosmosClient = cosmosClient;
        _logger = logger;
        
        var databaseName = configuration["CosmosDb:DatabaseName"] ?? throw new ArgumentException("CosmosDb:DatabaseName is required");
        var containerName = configuration["CosmosDb:ContainerName"] ?? throw new ArgumentException("CosmosDb:ContainerName is required");
        
        _container = _cosmosClient.GetContainer(databaseName, containerName);
    }
    
    public async Task<User?> GetUserAsync(long userId)
    {
        try
        {
            _logger.LogInformation($"Getting user with ID {userId}");
            
            var response = await _container.ReadItemAsync<User>(
                userId.ToString(), 
                new PartitionKey(userId)
            );
            
            _logger.LogInformation($"Successfully retrieved user {userId}");
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning($"User {userId} not found");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting user {userId}: {ex.Message}");
            throw;
        }
    }
    
    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        try
        {
            _logger.LogInformation("Getting all users");
            
            var query = new QueryDefinition("SELECT * FROM c WHERE c.type = 'user'");
            var users = new List<User>();
            
            var iterator = _container.GetItemQueryIterator<User>(query);
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                users.AddRange(response.ToList());
            }
            
            _logger.LogInformation($"Retrieved {users.Count} users");
            return users;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting all users: {ex.Message}");
            throw;
        }
    }
    
    public async Task<User> UpsertUserAsync(User user)
    {
        try
        {
            _logger.LogInformation($"Upserting user {user.UserId}");
            
            var response = await _container.UpsertItemAsync(
                user,
                new PartitionKey(user.UserId)
            );
            
            _logger.LogInformation($"Successfully upserted user {user.UserId}");
            return response.Resource;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error upserting user {user.UserId}: {ex.Message}");
            throw;
        }
    }
    
    public async Task<User?> AddFollowedPlayerAsync(long userId, FollowedPlayer followedPlayer)
    {
        try
        {
            _logger.LogInformation($"Adding followed player {followedPlayer.UserId} to user {userId}");
            
            var user = await GetUserAsync(userId);
            if (user == null)
            {
                _logger.LogWarning($"User {userId} not found");
                return null;
            }
            
            // Check if the player is already being followed
            var existingPlayer = user.Following.FirstOrDefault(f => f.UserId == followedPlayer.UserId);
            if (existingPlayer != null)
            {
                _logger.LogInformation($"Player {followedPlayer.UserId} is already being followed by user {userId}");
                return user;
            }
            
            user.Following.Add(followedPlayer);
            
            var response = await _container.ReplaceItemAsync(
                user,
                user.Id,
                new PartitionKey(user.UserId)
            );
            
            _logger.LogInformation($"Successfully added followed player {followedPlayer.UserId} to user {userId}");
            return response.Resource;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error adding followed player {followedPlayer.UserId} to user {userId}: {ex.Message}");
            throw;
        }
    }
    
    public async Task<bool> UpdateLastMatchIdAsync(long userId, long followedPlayerId, string lastMatchId)
    {
        try
        {
            _logger.LogInformation($"Updating last match ID for player {followedPlayerId} followed by user {userId}");
            
            var user = await GetUserAsync(userId);
            if (user == null)
            {
                _logger.LogWarning($"User {userId} not found");
                return false;
            }
            
            var followedPlayer = user.Following.FirstOrDefault(f => f.UserId == followedPlayerId);
            if (followedPlayer == null)
            {
                _logger.LogWarning($"User {userId} is not following player {followedPlayerId}");
                return false;
            }
            
            followedPlayer.LastMatchId = lastMatchId;
            
            await _container.ReplaceItemAsync(
                user,
                user.Id,
                new PartitionKey(user.UserId)
            );
            
            _logger.LogInformation($"Successfully updated last match ID for player {followedPlayerId} followed by user {userId}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating last match ID for player {followedPlayerId} followed by user {userId}: {ex.Message}");
            throw;
        }
    }
}

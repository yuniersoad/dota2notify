using Dota2Notify.Api.Models;
using Dota2Notify.Api.notifications;
using Dota2Notify.Api.Services;

namespace Dota2Notify.Api;

public class MatchCheckBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MatchCheckBackgroundService> _logger;
    private readonly TimeSpan _checkInterval;
    private readonly bool _isEnabled;
    
    public MatchCheckBackgroundService(IServiceProvider serviceProvider, ILogger<MatchCheckBackgroundService> logger, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        
        // Get check interval from configuration (default to 5 minutes)
        var intervalMinutes = configuration.GetValueWithEnvOverride("MatchCheck:IntervalMinutes");
        _checkInterval = TimeSpan.FromMinutes(int.TryParse(intervalMinutes, out var minutes) ? minutes : 5);
        
        // Check if the service is enabled (default to true)
        var isEnabledStr = configuration.GetValueWithEnvOverride("MatchCheck:Enabled");
        _isEnabled = !bool.TryParse(isEnabledStr, out var isEnabled) || isEnabled;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Match check service is starting. Enabled: {enabled}", _isEnabled);
        
        if (!_isEnabled)
        {
            _logger.LogInformation("Match check service is disabled in configuration. Service will not run.");
            return;
        }
        
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Running match check at {time}", DateTimeOffset.UtcNow);
            
            try
            {
                await CheckNewMatchesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while checking for new matches");
            }
            
            _logger.LogInformation("Match check completed. Next check in {minutes} minutes", _checkInterval.TotalMinutes);
            await Task.Delay(_checkInterval, stoppingToken);
        }
    }
    
    private async Task CheckNewMatchesAsync(CancellationToken stoppingToken)
    {
        // Create a new scope for each check to ensure fresh service instances
        using var scope = _serviceProvider.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        var dotaService = scope.ServiceProvider.GetRequiredService<IDotaService>();
        var notifyService = scope.ServiceProvider.GetRequiredService<INotifyService>();
        
        // Get all users
        var users = await userService.GetAllUsersAsync();
        _logger.LogInformation("Found {count} users to process", users.Count());
        
        foreach (var user in users)
        {
            if (stoppingToken.IsCancellationRequested)
                break;
                
            await ProcessUserAsync(user, userService, dotaService, notifyService);
            
        }
    }
    
    private async Task ProcessUserAsync(User user, IUserService userService, IDotaService dotaService, INotifyService notifyService)
    {
        _logger.LogInformation("Processing user {userId} ({name})", user.UserId, user.Name);
        
        foreach (var followedPlayer in user.Following)
        {
            _logger.LogInformation("Checking for new matches for player {playerId} ({name})", followedPlayer.UserId, followedPlayer.Name);
            
            // Get recent matches for this player
            var recentMatches = await dotaService.GetPlayerRecentMatchesAsync(followedPlayer.UserId, 1);
            
            if (recentMatches.First().MatchId == followedPlayer.LastMatchId)
            {
                _logger.LogInformation("No new matches found for player {playerId} (last match ID: {lastMatchId})", followedPlayer.UserId, followedPlayer.LastMatchId);
                continue;
            }
            
           var newestMatch = recentMatches.First();
            
            // Create notification message
            var outcome = newestMatch.PlayerWon ? "won" : "lost";
            var matchDuration = $"{newestMatch.MatchDuration.Minutes}m {newestMatch.MatchDuration.Seconds}s";
            var message = $"{followedPlayer.Name} {outcome} a match as {newestMatch.HeroName} " +
                          $"with KDA {newestMatch.Kills}/{newestMatch.Deaths}/{newestMatch.Assists}. " +
                          $"Match duration: {matchDuration}. " +
                          $"Match ID: {newestMatch.MatchId}";
            
            _logger.LogInformation("Sending notification to user {userName} (ID: {userId}): {message}", 
                user.Name, user.UserId, message);
            
            // Send notification
            await notifyService.SendNotificationAsync(message);
            
            // Update last seen match ID
            await userService.UpdateLastMatchIdAsync(user.UserId, followedPlayer.UserId, newestMatch.MatchId);
            
            _logger.LogInformation("Updated last match ID for player {playerId} to {matchId}", 
                followedPlayer.UserId, newestMatch.MatchId);
        }
    }
}

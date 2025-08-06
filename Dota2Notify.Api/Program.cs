using Dota2Notify.Api;
using Dota2Notify.Api.notifications;
using Dota2Notify.Api.Models;
using Dota2Notify.Api.Services;
using Microsoft.Azure.Cosmos;
using DotNetEnv;
using System.IO;
using OpenTelemetry.Metrics;
using System.Diagnostics.Metrics;
using Azure.Monitor.OpenTelemetry.Exporter;

// Load environment variables from .env file in development
// if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
// {
    Env.Load(Path.Combine(Directory.GetCurrentDirectory(), "..", ".env"));
    Env.TraversePath().Load(); // This will also look for .env files in parent directories
//}

var builder = WebApplication.CreateBuilder(args);

// Add OpenTelemetry metrics
var azureMonitorConnectionString = builder.Configuration.GetValueWithEnvOverride("AzureMonitor:ConnectionString");

builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics.AddMeter("Dota2Notify.Api");
        
        // Add console exporter for development
        metrics.AddConsoleExporter();
        
        // Add Azure Monitor exporter if connection string is provided
        if (!string.IsNullOrEmpty(azureMonitorConnectionString))
        {
            metrics.AddAzureMonitorMetricExporter(options =>
            {
                options.ConnectionString = azureMonitorConnectionString;
            });
            Console.WriteLine("Azure Monitor metrics exporter configured");
        }
        else
        {
            Console.WriteLine("Azure Monitor connection string not found. Only console exporter will be used.");
        }
    });

// Add services to the container.
builder.Services.AddHttpClient();
builder.Services.AddScoped<INotifyService, TelegramNotifyService>();
builder.Services.AddScoped<IDotaService, OpenDotaService>();

// Add Cosmos DB services
var cosmosDbEndpoint = builder.Configuration.GetValueWithEnvOverride("CosmosDb:EndpointUri");
var cosmosDbKey = builder.Configuration.GetValueWithEnvOverride("CosmosDb:PrimaryKey");
builder.Services.AddSingleton(s => new CosmosClient(cosmosDbEndpoint, cosmosDbKey));
builder.Services.AddScoped<IUserService, CosmosDbUserService>();

// Add match checking background service
builder.Services.AddHostedService<MatchCheckBackgroundService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapPost("/notify", async (long userId, string message, INotifyService notifyService, IUserService userService) =>
{
    try
    {
        var user = await userService.GetUserAsync(userId);
        if (user == null)
        {
            return Results.NotFound(new { success = false, error = $"User {userId} not found" });
        }
        
        await notifyService.SendNotificationAsync(message, user.TelegramChatId);
        return Results.Ok(new { success = true, message = "Notification sent successfully" });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { success = false, error = ex.Message });
    }
})
.WithName("SendNotification")
.WithOpenApi();

app.MapGet("/dota/matches/{playerId:long}", async (long playerId, int? limit, IDotaService dotaService) =>
{
    try
    {
        var matches = await dotaService.GetPlayerRecentMatchesAsync(playerId, limit ?? 10);
        return Results.Ok(matches);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { success = false, error = ex.Message });
    }
})
.WithName("GetPlayerMatches")
.WithOpenApi();

app.MapGet("/dota/matches", async (IConfiguration config, IDotaService dotaService) =>
{
    try
    {
        var defaultPlayerId = config.GetValue<long>("OpenDota:DefaultPlayerId");
        var matches = await dotaService.GetPlayerRecentMatchesAsync(defaultPlayerId);
        return Results.Ok(matches);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { success = false, error = ex.Message });
    }
})
.WithName("GetDefaultPlayerMatches")
.WithOpenApi();

// User endpoints
app.MapGet("/users", async (IUserService userService) =>
{
    try
    {
        var users = await userService.GetAllUsersAsync();
        return Results.Ok(users);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { success = false, error = ex.Message });
    }
})
.WithName("GetAllUsers")
.WithOpenApi();

app.MapGet("/users/{userId}", async (long userId, IUserService userService) =>
{
    try
    {
        var user = await userService.GetUserAsync(userId);
        if (user == null)
        {
            return Results.NotFound(new { success = false, error = $"User {userId} not found" });
        }
        return Results.Ok(user);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { success = false, error = ex.Message });
    }
})
.WithName("GetUser")
.WithOpenApi();

// Match checking is now handled by the background service

app.Run();
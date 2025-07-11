using Dota2Notify.Api.notifications;
using Dota2Notify.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpClient();
builder.Services.AddScoped<INotifyService, TelegramNotifyService>();
builder.Services.AddScoped<IDotaService, OpenDotaService>();

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

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.MapPost("/notify", async (INotifyService notifyService, string message) =>
{
    try
    {
        await notifyService.SendNotificationAsync(message);
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

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

# Dota2Notify

A .NET API to notify users about recent Dota 2 matches played by followed players via Telegram.

## Features

- Get recent matches for Dota 2 players using the OpenDota API
- Send notifications through Telegram
- Store users and their followed players in Azure Cosmos DB
- Track the last match ID for each followed player

## Requirements

- .NET 8.0 SDK
- Azure Cosmos DB account (or [Azure Cosmos DB Emulator](https://docs.microsoft.com/en-us/azure/cosmos-db/local-emulator) for local development)
- Telegram bot token

## Getting Started

1. Clone the repository
2. Update `appsettings.json` with your configuration values:
   - Telegram bot token and chat ID
   - Azure Cosmos DB connection details

## Local Development

When running locally, the application will use the Cosmos DB Emulator by default. Make sure you have it installed and running before starting the application.

## API Endpoints

### Dota Match Endpoints
- `GET /dota/matches/{playerId}?limit=10` - Get recent matches for a specific player
- `GET /dota/matches` - Get matches for the default player

### User Endpoints
- `GET /users` - Get all users
- `GET /users/{userId}` - Get a specific user

### Notification Endpoint
- `POST /notify` - Send a Telegram notification

## Configuration

The application uses the following configuration sections:

```json
{
  "Telegram": {
    "BotToken": "your-telegram-bot-token",
    "ChatId": "your-telegram-chat-id"
  },
  "OpenDota": {
    "BaseUrl": "https://api.opendota.com/api",
    "DefaultPlayerId": 123456789
  },
  "CosmosDb": {
    "EndpointUri": "https://your-cosmos-db-account.documents.azure.com:443/",
    "PrimaryKey": "your-cosmos-db-primary-key",
    "DatabaseName": "dota2notify",
    "ContainerName": "users"
  }
}
```
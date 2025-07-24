# Dota2Notify

A .NET API to notify users about recent Dota 2 matches played by followed players via Telegram.

## Features

- Get recent matches for Dota 2 players using the OpenDota API
- Send notifications through Telegram
- Store users and their followed players in Azure Cosmos DB
- Track the last match ID for each followed player
- Automatically check for new matches at configurable intervals
- Send notifications when followed players play new matches

## Requirements

- .NET 8.0 SDK
- Azure Cosmos DB account (or [Azure Cosmos DB Emulator](https://docs.microsoft.com/en-us/azure/cosmos-db/local-emulator) for local development)
- Telegram bot token

## Getting Started

1. Clone the repository
2. Set up your environment:
   - Option 1: Create a `.env` file based on `.env.example`
   - Option 2: Update `appsettings.json` with your configuration values

## Local Development

When running locally, the application will:
1. Load environment variables from the `.env` file in the root directory
2. Use the Cosmos DB Emulator by default (make sure you have it installed and running)

### Using Environment Variables

The application supports environment variables to override configuration. For local development:

1. Copy `.env.example` to `.env`: `cp .env.example .env`
2. Edit `.env` with your actual values
3. The application will automatically load these values when starting

## API Endpoints

### Dota Match Endpoints
- `GET /dota/matches/{playerId}?limit=10` - Get recent matches for a specific player
- `GET /dota/matches` - Get matches for the default player

### User Endpoints
- `GET /users` - Get all users
- `GET /users/{userId}` - Get a specific user

### Notification Endpoint
- `POST /notify` - Send a Telegram notification

### Automatic Match Checking
The application includes a background service that periodically checks for new matches based on the configured interval in `MatchCheck:IntervalMinutes`.

## Configuration

The application uses the following configuration sections:

```json
{
  "Telegram": {
    "BotToken": "your-telegram-bot-token",
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
  },
  "MatchCheck": {
    "IntervalMinutes": 5,
    "Enabled": true
  }
}

```
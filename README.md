# JobFinder

JobFinder is a .NET worker service that monitors JustJoin.it job offers and sends new matching offers to a Telegram chat.

The service currently searches for .NET offers around Warsaw, stores already sent offer slugs in a local SQLite database, and avoids sending duplicates. It also starts a Telegram bot listener with an inline button that can manually fetch the latest 20 offers.

## Features

- Background worker built with the .NET Worker Service template.
- JustJoin.it offer fetching with `Flurl.Http`.
- Telegram notifications through `Telegram.Bot`.
- SQLite persistence with Entity Framework Core.
- EF Core migrations for the `SentOffers` table.
- Automatic polling every 5 minutes.

## Requirements

- .NET 10 SDK
- A Telegram bot token from BotFather
- A Telegram chat ID for the destination chat

## Configuration

Configure the Telegram settings with .NET User Secrets during local development:

```powershell
dotnet user-secrets set "Telegram:BotToken" "your-bot-token" --project .\JustJoinJobFinder\JustJoinJobFinder.csproj
dotnet user-secrets set "Telegram:ChatId" "your-chat-id" --project .\JustJoinJobFinder\JustJoinJobFinder.csproj
```

For production, provide the same values through environment variables or your hosting platform secret manager:

```text
Telegram__BotToken=your-bot-token
Telegram__ChatId=your-chat-id
```

## Running Locally

Restore dependencies:

```powershell
dotnet restore
```

Run the worker:

```powershell
dotnet run --project .\JustJoinJobFinder\JustJoinJobFinder.csproj
```

The application applies EF Core migrations automatically on startup and creates a local `offers.db` file in the runtime output directory.

## Project Structure

```text
JustJoinJobFinder/
  DB/              Entity Framework Core database context and entities
  Migrations/      EF Core migrations
  Models/          JustJoin.it API response models
  Worker.cs        Background worker, Telegram bot, and offer polling logic
  Program.cs       Application entry point
```

## Notes

- Build outputs, Visual Studio metadata, local SQLite databases, logs, and local environment files are ignored by Git.
- Do not commit real Telegram credentials. Use User Secrets locally and environment variables in deployed environments.

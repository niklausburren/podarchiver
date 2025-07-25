# PodArchiver

A lightweight and reliable podcast download service built with .NET 8.  
PodArchiver fetches episodes from RSS feeds, stores them in a structured way, tags audio files, and optionally cleans up old episodes.

## Features

- Automatic download of podcast episodes from any RSS feed
- Configurable number of episodes to keep per feed
- Custom podcast naming (optional in `config.json`)
- Tagging of downloaded audio files (title, artist, cover, etc.)
- Automatic cleanup of old episodes
- Multiple feeds and download times configurable
- Logging via NLog

## Installation

1. Install [.NET 8 SDK](https://dotnet.microsoft.com/download)
2. Clone the repository: `git clone https://github.com/niklausburren/podarchiver.git`
3. Restore dependencies: `dotnet restore`

## Configuration

Edit the `config.json` file in the main directory:

```json
{
  "outputPath": "downloads",
  "downloadTimes": ["06:00", "18:00"],
  "feeds": [
	{
	  "url": "https://example.com/feed.xml",
	  "count": 10,
	  "name": "My Favorite Podcast"
	},
	{
	  "url": "https://another-feed.com/rss"
	  // No name: RSS feed title will be used
	}
  ]
}
```

- `outputPath`: Target folder for downloads
- `downloadTimes`: Times of day for automatic downloads (optional)
- `feeds`: List of RSS feeds, each with URL, optional episode count, and optional name

If a `name` is specified for a feed, it will be used for folder structure and tagging. Otherwise, the name from the RSS feed will be used.

## Usage

Run the app with:

`dotnet run --project PodArchiver`

Episodes will be saved in the specified output folder, organized by podcast and year.

## Logging

Logging is handled via NLog. Configuration can be found in `NLog.config`.

## Project Structure

```
PodArchiver/
├── Models/         # Data models (FeedConfig, AppConfig, PodcastChannel, PodcastEpisode)
├── Services/       # Main logic (PodArchiver, RssParser, PodArchiverService)
├── Tagging/        # TagWriter for audio file tagging
├── Utils/          # Utility functions
├── config.json     # Configuration file
├── NLog.config     # Logging configuration
└── Program.cs      # Application entry point
```

## License

This project is licensed under the MIT License.

---

**Note:**  
If a `name` is specified for a feed in `config.json`, it will be used for folder names and tagging. Otherwise, the name from the RSS feed will be used.

---

Enjoy archiving your favorite podcasts!
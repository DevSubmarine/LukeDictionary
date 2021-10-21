# Luke Dictionary Bot
[![GitHub top language](https://img.shields.io/github/languages/top/DevSubmarine/LukeDictionary)](https://github.com/DevSubmarine/LukeDictionary) [![GitHub](https://img.shields.io/github/license/DevSubmarine/LukeDictionary)](LICENSE) [![GitHub Workflow Status](https://img.shields.io/github/workflow/status/DevSubmarine/LukeDictionary/.NET%20Build)](https://github.com/DevSubmarine/LukeDictionary/actions) [![GitHub issues](https://img.shields.io/github/issues/DevSubmarine/LukeDictionary)](https://github.com/DevSubmarine/LukeDictionary/issues)

`!lukeadd <word>` was a running joke for a long while now. Let's make the joke real.

This is a Discord bot that will allow people add Luke's misspellings to database, and then retrieve them.

## Creating bot
To create the bot, go to [Discord Developer Portal](https://discord.com/developers/applications/) and create a new application. Make sure to grab Token from `Bot` tab, as it'll be needed in steps [below](#running).

To add the bot to the guild (server), go to `OAuth2` tab, and create a new link. Make sure to select both `bot` and `applications.commands` scopes.

## Running
Pre-requirements: 
- [.NET 5](https://dotnet.microsoft.com/download/dotnet/5.0).
- MongoDB Cluster.

1. Download or clone.
2. Run [Database Bootstrapper](Tools/DatabaseBootstrapper) tool to create MongoDB collections.
3. Add `appsecrets.json` file ("Content" for **Build Action**, and "Copy always" or "Copy if newer" for **Copy to Output Directory**).
4. Populate with secrets. See [appsecrets.Example.json](LukeDictionary.Bot/appsecrets.Example.json) for example.
5. Ensure IP Address of the host that will run the bot is whitelisted in your MongoDB cluster.
6. Build and run.

## License
Copyright (c) 2021 DevSubmarine & TehGM

Licensed under [Apache 2.0 License](LICENSE).
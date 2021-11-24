# Discord Crypto Sidebar Bot

## Pre-requisites:
.NET 6 see Step 2 [here](https://docs.microsoft.com/en-us/dotnet/iot/deployment).

## How to use
### Build the project first using

    dotnet build --configuration=Release

### Run using:

    dotnet run --no-build --configuration=Release --updateInterval 60 --botToken [Discord Bot Token Here] --roleId [Member Role Id] --guildId [Guild Id] --timelimit [In minutes] --kickchannelid [kick log channel id]

### or with PM2:

    pm2 start "dotnet run --no-build --configuration=Release --updateInterval 60 --botToken [Discord Bot Token Here] --roleid [Member Role Id] --guildId [Guild Id] --timelimit [In minutes] --kickchannelid [kick log channel id]
" --name DoveRoleBot

## How to update:
### Standalone:

    git pull && dotnet build

### or with PM2:

    git pull && pm2 stop all && dotnet build && pm2 restart all

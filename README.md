# Diffcord

## Configuration

Use `.env` or Docker environment variable to set Discord Bot Tokey in `DISCORD_BOT_TOKEN`.

## Command

All of command should starts with `+=`.

There are commands

- logging subsystem edit \[yes/no] **# Enable or Disable edit logging**
- logging subsystem delete \[yes/no] **# Enable or Disable deletion logging**
- logging here **# Set logging channel here**
- logging destination <channel id> **# Set logging channel**
- write **# Write configuration to file**
- show running-config **# Show current config**

## Config file

We use `./config.yaml` to save informations.

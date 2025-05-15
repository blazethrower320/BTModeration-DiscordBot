# BTModeration Discord Bot

This is a custom Discord bot built using [Discord.Net](https://github.com/discord-net/Discord.Net) that integrates directly with the [BTModeration Plugin](https://imperialplugins.com/Unturned/Products/BTModeration) for Unturned.

The bot is a free addon for BTModeration listed on ImperialPlugins.com. It allows staff to view Profiles, Punishments and see recent username changes of a user all from discord!

## 🔧 Features

### 👥 User Commands
- `/link <code>`  
  Allows players to link their Discord account with their in-game profile using a unique code provided in-game. (Requires [BTLink](https://github.com/BTPlugins/BTLink) to work )

### 🛠️ Staff Commands
- `/recent-usernames <steam_id>`  
  View the recent usernames associated with a specific Steam ID.
  
- `/profile <steam_id | @discord_user>`  
  Fetch detailed profile information for a player using either their Steam ID or linked Discord account.

## ⚙️ Configuration

The bot uses a simple YAML-based configuration for setup. Below is a sample `config.yml`:

```yaml
client:
  token: YourToken  
  accessRole: Staff Team
  linkedRole: Linked

database:
  host: 127.0.0.1
  port: 3306
  username: root
  password: password
  name: unturned    

dev:
  debug: true

using BTModeration_DiscordBot.Helpers;
using BTModeration_DiscordBot.Services;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace BTModeration_DiscordBot.Modules
{
    //[Discord.Interactions.Group("staff", "things for Staff members.")]

    public class MStaff : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly MySqlDBService _dbService;
        private readonly IConfigurationRoot _configuration;

        public MStaff(MySqlDBService dbService, IConfigurationRoot Config)
        {
            this._dbService = dbService;
            this._configuration = Config;
        }
        [SlashCommand("recent-usernames", "Displays the Recent usernames of a player")]
        public async Task recentusernames(string SteamID)
        {
            var user = Context.User as SocketGuildUser;
            if(user.Roles.FirstOrDefault(c => c.Name.Equals(_configuration["client:accessRole"])) == null)
            {
                // Dont have role
                var embedBuild = new EmbedBuilder()
                {
                    Title = "Missing Role",
                    Description = $"You are missing {_configuration["client:accessRole"]} Role!",
                    Color = Color.Red,
                    Footer = new EmbedFooterBuilder { Text = "BTModeration • " + DateTime.Now.ToString("dddd, dd MMMM yyyy") },
                };
                await RespondAsync(embed: embedBuild.Build());
                return;
            }


            if (!ulong.TryParse(SteamID, out var steamID))
            {
                var embedBuild = new EmbedBuilder()
                {
                    Title = "Invalid Steam ID",
                    Description = "Please enter a valid Steam 64 ID!",
                    Color = Color.Red,
                    Footer = new EmbedFooterBuilder { Text = "BTModeration • " + DateTime.Now.ToString("dddd, dd MMMM yyyy") },
                };
                await RespondAsync(embed: embedBuild.Build());
                return;
            }
            var player = await _dbService.GetPlayerInformation(steamID);

            if (player == null)
            {
                var embedBuild = new EmbedBuilder()
                {
                    Title = "Invalid Steam ID",
                    Description = $"{steamID} was not found in the Database.",
                    Color = Color.Red,
                    Footer = new EmbedFooterBuilder { Text = "BTModeration • " + DateTime.Now.ToString("dddd, dd MMMM yyyy") },
                };
                await RespondAsync(embed: embedBuild.Build());
                return;
            }

            var usernames = await _dbService.GetPlayerRecentUsernames(steamID);
            if(usernames == null)
            {
                var newEmbed = new EmbedBuilder()
                {
                    Title = "No Recent Usernames",
                    Description = $"{player.Username} does not have any recent username changes!",
                    Color = Color.Red,
                };
                await RespondAsync(embed:  newEmbed.Build());
                return;
            }
            var UsernamesDescription = "";
            for (int i = 1; i <= usernames.Count; i++)
            {
                UsernamesDescription += $"**Username Change #{i}:**\n> **New Username:** {usernames[i-1].NewUsername}\n> **Old Username:** {usernames[i-1].OldUsername}\n> **Date:** {usernames[i-1].Date.ToString("dddd, dd MMMM yyyy")}\n\n";
            }
            var usernameEmbed = new EmbedBuilder()
            {
                Title = $"{player.Username} Recent Usernames",
                Url = "https://steamcommunity.com/profiles/" + player.PlayerID,
                Description = UsernamesDescription,
                Color = Color.DarkMagenta,
                Footer = new EmbedFooterBuilder { Text = "BTModeration • " + DateTime.Now.ToString("dddd, dd MMMM yyyy") },
            };
            await RespondAsync(embed: usernameEmbed.Build());

        }

        [SlashCommand("profile", "Gives Information on the player")]
        public async Task profile([Discord.Interactions.Summary(description: "The user to target, if any.")] SocketGuildUser user = null,
        [Discord.Interactions.Summary(description: "The input string, if any.")] string SteamID = null)
        {
            var user2 = Context.User as SocketGuildUser;
            if (user2.Roles.FirstOrDefault(c => c.Name.Equals(_configuration["client:accessRole"])) == null)
            {
                // Dont have role
                var embedBuild = new EmbedBuilder()
                {
                    Title = "Missing Role",
                    Description = $"You are missing {_configuration["client:accessRole"]} Role!",
                    Color = Color.Red,
                    Footer = new EmbedFooterBuilder { Text = "BTModeration • " + DateTime.Now.ToString("dddd, dd MMMM yyyy") },
                };
                await RespondAsync(embed: embedBuild.Build());
                return;
            }


            if (SteamID == null && user == null || SteamID != null && user != null)

            {
                var newEmbed = new EmbedBuilder()
                {
                    Title = "Missing Argument",
                    Description = "Please enter either a Steam 64 ID or a @Discord User!",
                    Color = Color.Red,
                };
                await RespondAsync(embed:  newEmbed.Build());
                return;
            }
            String stringSteamID = "";

            if (user != null)
            {
                var linkedAccount = await _dbService.GetPlayerLinkedAccount(user.Id);
                if (linkedAccount == null)
                {
                    var newEmbed = new EmbedBuilder()
                    {
                        Title = "User is not linked",
                        Description = "The discord User has not linked their steam account!",
                        Color = Color.Red,
                    };
                    await RespondAsync(embed: newEmbed.Build());
                    return;
                }
                Console.WriteLine("PlayerID: " + linkedAccount.PlayerID);
                stringSteamID = linkedAccount.PlayerID.ToString();
            }
            else
                stringSteamID = SteamID;

            if (!ulong.TryParse(stringSteamID, out var steamID))
            {
                var embedBuild = new EmbedBuilder()
                {
                    Title = "Invalid Steam ID",
                    Description = "Please enter a valid Steam 64 ID!",
                    Color = Color.Red,
                    Footer = new EmbedFooterBuilder { Text = "BTModeration • " + DateTime.Now.ToString("dddd, dd MMMM yyyy") },
                };
                await RespondAsync(embed: embedBuild.Build());
                return;
            }
            var player = await _dbService.GetPlayerInformation(steamID);

            if(player == null)
            {
                var embedBuild = new EmbedBuilder()
                {
                    Title = "Invalid Steam ID",
                    Description = $"{steamID} was not found in the Database.",
                    Color = Color.Red,
                    Footer = new EmbedFooterBuilder { Text = "BTModeration • " + DateTime.Now.ToString("dddd, dd MMMM yyyy") },
                };
                await RespondAsync(embed: embedBuild.Build());
                return;
            }

            
            try
            {
                var warnings = await _dbService.GetPlayerWarningsCount(player.PlayerID);
                var notations = await _dbService.GetPlayerNotationsCount(player.PlayerID);
                var kicks = await _dbService.GetPlayerKicksCount(player.PlayerID);
                var bans = await _dbService.GetPlayerBansCount(player.PlayerID);
                var commendations = await _dbService.GetPlayerCommendationsCount(player.PlayerID);

                var embed = new EmbedBuilder
                {
                    Title = $"Record Found for {player.Username}!",
                    Url = "https://steamcommunity.com/profiles/" + player.PlayerID,
                    Color = Color.LightOrange,
                    Description = $"Record Requested by Staff Member: " +
                        $"\n{Context.User.Mention}" +
                        $"\n\n**Player Information**:" +
                        $"\n[Steam Profile](https://steamcommunity.com/profiles/{player.PlayerID}) | [Steam Avatar]({player.ProfileImage.ToString()})" +
                        $"\nSteamID: ``{player.PlayerID}``" +
                        $"\nPlayer Last Seen: ``{player.LastPlayed.ToString("dddd, dd MMMM yyyy")}``" +
                        $"\nPlayer First Joined: ``{player.FirstPlayed.ToString("dddd, dd MMMM yyyy")}``" +
                        $"\nPlayTime: ``" + TimeConverterHelper.Format(TimeSpan.FromSeconds(player.PlayTime), 2) + "``",
                    Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder
                    {
                        Name = "**Notation**",
                        Value = notations,
                        IsInline = true,
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "**Warnings**",
                        Value = warnings,
                        IsInline = true,
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "**Kicks**",
                        Value = kicks,
                        IsInline = true,
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "**Bans**",
                        Value = bans,
                        IsInline = true,
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "**Commendations**",
                        Value = commendations,
                        IsInline = true,
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "**Trustscore**",
                        Value = player.Trustscore.ToString(),
                        IsInline = true,
                    },
                },
                    Footer = new EmbedFooterBuilder
                    {
                        Text = "BTModeration • " + DateTime.Now.ToString("dddd, dd MMMM yyyy")
                    },
                };
                await DeferAsync();

                IUserMessage message = await Context.Channel.SendMessageAsync(embed: embed.Build());
                await message.AddReactionAsync(new Emoji("📓"));
                await message.AddReactionAsync(new Emoji("⚠️"));
                await message.AddReactionAsync(new Emoji("👢"));
                await message.AddReactionAsync(new Emoji("🔨"));
                await message.AddReactionAsync(new Emoji("👏"));
                await DeleteOriginalResponseAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex}");
            }

        }
    }
}

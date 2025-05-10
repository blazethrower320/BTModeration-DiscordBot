using BTModeration_DiscordBot.Helpers;
using BTModeration_DiscordBot.Interfaces;
using BTModeration_DiscordBot.Models;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BTModeration_DiscordBot.Services
{
    public class ReactionService
    {
        private readonly DiscordSocketClient _client;
        private readonly IServiceProvider _services;
        private readonly MySqlDBService _dbService;
        private readonly IConfigurationRoot _configuration;

        public ReactionService(DiscordSocketClient client, IServiceProvider services, MySqlDBService dbService, IConfigurationRoot config)
        {
            _client = client;
            _services = services;
            _dbService = dbService;
            _configuration = config;
            _client.ReactionAdded += OnReactionAdded;
        }

        private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> cacheable1, Cacheable<IMessageChannel, ulong> cacheable2, SocketReaction reaction)
        {
            Console.WriteLine("Trigger");
            if (!reaction.Message.IsSpecified || reaction.User.Value == null)
            {
                Console.WriteLine("Message or user value is not set...");
                return;
            }
            if (reaction.Message.Value is not IUserMessage socketMessage || reaction.User.Value.IsBot)
            {
                Console.WriteLine("Not a valid message or bot reacted...");
                return;
            }

            if (socketMessage.Embeds.Count <= 0)
            {
                Console.WriteLine("Not a Embed");
                return;
            }

            if (!socketMessage.Embeds.First().Url.Contains("https://steamcommunity.com/profiles/"))
            {
                // Not a BTModeration
                Console.WriteLine("Not a BTModeration Embed");
                return;
            }


            var reactionCheck = await socketMessage.GetReactionUsersAsync(reaction.Emote, 100).FlattenAsync();
            var isBotReacted = reactionCheck.Any(c => c.IsBot);

            if (!isBotReacted)
            {
                Console.WriteLine("Bot has not Reacted");
                return;
            }

            var channel = reaction.Channel as ITextChannel;
            if (channel == null)
            {
                Console.WriteLine("Channel == null");
                return;
            }
            
            /*
            var user = reaction.User.Value as SocketGuildUser;
            var hasRole = user.Roles.FirstOrDefault(c => c.Id.Equals(756719010568994817));
            if (hasRole == null)
            {
                // Does not have the role to add reaction to UI
                Console.WriteLine("Missing Role to get information");
                var newEmbed = new EmbedBuilder()
                {
                    Title = "Missing Permission",
                    Description = $"You are missing <@756719010568994817> Role to use this command",
                    Color = Color.Red,
                    Footer = new EmbedFooterBuilder { Text = "BTModeration • " + DateTime.Now.ToString("dddd, dd MMMM yyyy") }
                    
                };
                await channel.SendMessageAsync(embed: newEmbed.Build());
                return;
            }
            */
            


            Console.WriteLine("Down Here");
            var embed = socketMessage.Embeds.First();

            var embedTitle = embed.Url;
            var pattern = @"https:\/\/steamcommunity\.com\/profiles\/(\d+)";
            var regex = new Regex(pattern);
            var match = regex.Match(embed.Url);
            if (!match.Success)
            {
                Console.WriteLine("Steam ID not found...");
                return;
            }

            if (!ulong.TryParse(match.Groups[1].Value, out ulong playerID))
            {
                Console.WriteLine("Invalid Steam ID...");
                return;
            }

            var targetPlayer = await _dbService.GetPlayerInformation(playerID);

            // Check Emojies
            if (reaction.Emote.Equals(new Emoji("📓")))
            {
                await HandleReactionAsync(playerID, "Notation", await _dbService.GetPlayerNotations(playerID), Color.Blue, channel, targetPlayer);
            }
            else if (reaction.Emote.Equals(new Emoji("⚠️")))
            {
                await HandleReactionAsync(playerID, "Warning", await _dbService.GetPlayerWarnings(playerID), Color.DarkOrange, channel, targetPlayer);
            }
            else if (reaction.Emote.Equals(new Emoji("👢")))
            {
                await HandleReactionAsync(playerID, "Kick", await _dbService.GetPlayerKicks(playerID), Color.Purple, channel, targetPlayer);
            }
            else if (reaction.Emote.Equals(new Emoji("👏")))
            {
                await HandleReactionAsync(playerID, "Commendation", await _dbService.GetPlayerCommendations(playerID), Color.Green, channel, targetPlayer);
            }
            else if (reaction.Emote.Equals(new Emoji("🔨")))
            {
                var bans = await _dbService.GetPlayerBans(playerID);
                if (bans.Count == 0)
                {
                    var noPunishments = new EmbedBuilder()
                    {
                        Color = Discord.Color.Red,
                        Title = $"Ban Information",
                        Description = "None on Record",
                    };
                    await channel.SendMessageAsync(embed: noPunishments.Build());
                    return;
                }


                var newEmbed = new EmbedBuilder()
                {
                    Title = $"{targetPlayer.Username} Bans",
                    Color = Color.Red,
                    Footer = new EmbedFooterBuilder { Text = "BTModeration • " + DateTime.Now.ToString("dddd, dd MMMM yyyy") }
                };
                var Embeddescription = "";

                for (int i = 1; i <= bans.Count; i++)
                {
                    Embeddescription += $"**Ban #{i}:**\n> **Reason:** {bans[i - 1].Reason}\n> **Moderator:** {bans[i - 1].ModeratorID}\n>**Ban Duration:** {TimeConverterHelper.Format(TimeSpan.FromSeconds(bans[i - 1].BanLength), 2)}\n> **Date:** {bans[i - 1].TimeBanned.ToString("dddd, dd MMMM yyyy")}\n\n";
                }
                newEmbed.Description = Embeddescription;
                await channel.SendMessageAsync(embed: newEmbed.Build());
            }
            else
            {
                await channel.SendMessageAsync("There has been an error.... Emojie not found");
            }


            Console.WriteLine("SteamID: " + playerID);
            //await channel.SendMessageAsync("SteamID: " + playerID);
        }

        private static async Task HandleReactionAsync<T>(ulong playerID, string punishmentType, List<T> playerPunishments, Discord.Color color, ITextChannel channel, Player targetPlayer) where T : IPlayerRecord
        {
            {
                if (playerPunishments.Count == 0)
                {
                    var noPunishments = new EmbedBuilder()
                    {
                        Color = color,
                        Title = $"{punishmentType} Information",
                        Description = "None on Record",
                    };
                    await channel.SendMessageAsync(embed: noPunishments.Build());
                    return;
                }

                var newEmbed = new EmbedBuilder()
                {
                    Title = $"{targetPlayer.Username} {punishmentType}",
                    Color = color,
                    Footer = new EmbedFooterBuilder { Text = "BTModeration • " + DateTime.Now.ToString("dddd, dd MMMM yyyy") }
                };
                var Embeddescription = "";

                for (int i = 1; i <= playerPunishments.Count; i++)
                {
                    Embeddescription += $"**{punishmentType} #{i}:**\n> **Reason:** {playerPunishments[i - 1].Reason}\n> **Moderator:** {playerPunishments[i - 1].ModeratorID}\n> **Date:** {playerPunishments[i - 1].Issued.ToString("dddd, dd MMMM yyyy")}\n\n";
                }
                newEmbed.Description = Embeddescription;
                await channel.SendMessageAsync(embed: newEmbed.Build());
            }
        }
    }
}

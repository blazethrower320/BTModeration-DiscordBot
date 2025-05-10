using BTModeration_DiscordBot.Services;
using Discord;
using Discord.Interactions;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace BTModeration_DiscordBot.Modules
{
    public class MClient : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly MySqlDBService _dbService;
        private readonly IConfigurationRoot _configuration;

        public MClient(MySqlDBService dbService, IConfigurationRoot config)
        {
            this._dbService = dbService;
            this._configuration = config;
        }

        [SlashCommand("link", "Links your Steam Account to your Discord Account")]
        public async Task link(string code)
        {
            try
            {
                var codeExists = await _dbService.GetPlayerLinkedAccount(code);
                if (codeExists == null)
                {
                    var noCode = new EmbedBuilder()
                    {
                        Title = "Code Not Found",
                        Description = "The following code you entered was not found!",
                        Color = Color.Red,
                        Footer = new EmbedFooterBuilder { Text = "BTModeration • " + DateTime.Now.ToString("dddd, dd MMMM yyyy") },
                    };
                    await RespondAsync(embed: noCode.Build());
                    return;
                }
                if (codeExists.Linked)
                {
                    var AlreadylinkedEmbed = new EmbedBuilder()
                    {
                        Title = "Already Linked",
                        Description = "This Code is already linked to a Discord Account!",
                        Color = Color.Red,
                        Footer = new EmbedFooterBuilder { Text = "BTModeration • " + DateTime.Now.ToString("dddd, dd MMMM yyyy") },
                    };
                    await RespondAsync(embed: AlreadylinkedEmbed.Build());
                    return;
                }

                await _dbService.LinkAccount(codeExists.PlayerID, Context.User.Id);
                var linkedEmbed = new EmbedBuilder()
                {
                    Title = "Successfully Linked",
                    Description = $"You have successfully linked your Discord Account to ``{codeExists.PlayerID}``",
                    Color = Color.Green,
                    Footer = new EmbedFooterBuilder { Text = "BTModeration • " + DateTime.Now.ToString("dddd, dd MMMM yyyy") },
                };
                await RespondAsync(embed: linkedEmbed.Build());
                var role = Context.Guild.Roles.FirstOrDefault(x => x.Name == _configuration["client:linkedRole"]);
                await (Context.User as IGuildUser).AddRoleAsync(role);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            

        }
    }
}

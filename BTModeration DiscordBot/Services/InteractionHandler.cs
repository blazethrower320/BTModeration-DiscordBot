using Discord.Interactions;
using Discord.WebSocket;
using Discord;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BTModeration_DiscordBot.Services
{
    public class InteractionHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _interactionService;
        private readonly IServiceProvider _serviceProvider;

        public InteractionHandler(DiscordSocketClient client, InteractionService interactionService, IServiceProvider serviceProvider)
        {
            _client = client;
            _interactionService = interactionService;
            _serviceProvider = serviceProvider;

            _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);
            _client.InteractionCreated += OnInteractionAsync;
        }

        private async Task OnInteractionAsync(SocketInteraction arg)
        {
            try
            {
                var ctx = new SocketInteractionContext(_client, arg);
                await _interactionService.ExecuteCommandAsync(ctx, _serviceProvider);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}

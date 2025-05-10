using BTModeration_DiscordBot.Services;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using ShimmyMySherbet.MySQL.EF.Core;
using Discord.Interactions;
using System.Windows.Input;
using MySql.Data.MySqlClient.Memcached;
using MySqlX.XDevAPI;
using Discord.Rest;
using System.Globalization;
using System.Reflection;
using Microsoft.Extensions.Hosting;
using Discord.Commands;
using System.Text.RegularExpressions;
using BTModeration_DiscordBot.Helpers;
using Rocket.Core.Utils;
using System.Threading.Channels;
using BTModeration_DiscordBot.Interfaces;
using BTModeration_DiscordBot.Models;

public class BTModerationBot
{
    public static IHost _hostService;
    private static void Main() => new BTModerationBot().MainAsync().GetAwaiter().GetResult();

    private async Task MainAsync()
    {
        try
        {
            Console.WriteLine("Starting BTModerationBot...");

            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddYamlFile("configuration.yaml", optional: false, reloadOnChange: true)
                .Build();

            _hostService = Host.CreateDefaultBuilder()
                .ConfigureServices((_, services) => services
                .AddSingleton(config)
                .AddSingleton<InteractionHandler>()
                .AddSingleton<ReactionService>()
                .AddSingleton<MySQLEntityClient>(sp =>
                    new MySQLEntityClient(
                        config["database:host"],
                        config["database:username"],
                        config["database:password"],
                        config["database:name"],
                        int.Parse(config["database:port"]!)))
                .AddSingleton<MySqlDBService>()
                .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
                .AddSingleton(new CommandService(new CommandServiceConfig()))
                .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
                {
                    GatewayIntents = GatewayIntents.All,
                    AlwaysDownloadUsers = true,
                    MessageCacheSize = 500
                })))
                .Build();

            Console.WriteLine("Configuration loaded.");
            await RunAsync(_hostService);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fatal error: {ex}");
        }
    }

    private async Task RunAsync(IHost host)
    {
        using IServiceScope serviceScope = host.Services.CreateAsyncScope();
        var provider = serviceScope.ServiceProvider;

        var client = provider.GetRequiredService<DiscordSocketClient>();
        var commands = provider.GetRequiredService<InteractionService>();
        var config = provider.GetRequiredService<IConfigurationRoot>();
        var interactionProviderService = provider.GetRequiredService<InteractionHandler>();
        var mySqlDbService = provider.GetRequiredService<MySqlDBService>();
        var reactionService = provider.GetRequiredService<ReactionService>();

        client.Log += LogAsync;
        client.Ready += async () =>
        {
            Console.WriteLine("Bot is ready!");
            await commands.RegisterCommandsGloballyAsync();
            await client.SetActivityAsync(new Game("BTModeration", ActivityType.Watching));
        };

        Console.WriteLine("Logging in...");
        await client.LoginAsync(TokenType.Bot, config["client:token"]);
        await client.StartAsync();
        Console.WriteLine("Bot started.");

        await Task.Delay(-1);
    }

    private Task LogAsync(LogMessage message)
    {
        Console.WriteLine($"[Discord] {message}");
        return Task.CompletedTask;
    }
}

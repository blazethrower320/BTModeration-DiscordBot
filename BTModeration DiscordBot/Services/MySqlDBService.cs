using BTModeration_DiscordBot.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ShimmyMySherbet.MySQL.EF.Core;
using System.Configuration;


namespace BTModeration_DiscordBot.Services
{
    public class MySqlDBService
    {
        private MySQLEntityClient _entityClient;
        private readonly IConfigurationRoot _configuration;


        public MySqlDBService(MySQLEntityClient entityClient, IConfigurationRoot config)
        {
            _entityClient = entityClient;
            _configuration = config;

            _ = ConnectAsync();
        }

        private async Task<bool> ConnectAsync()
        {
            try
            {
                var ec = new MySQLEntityClient(
                    _configuration["database:host"],
                    _configuration["database:username"], 
                    _configuration["database:password"],
                    _configuration["database:name"],
                    int.Parse(_configuration["database:port"]!));

                _entityClient = ec;

                return ec.ConnectAsync().Result;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
                return false;
            }
        }
        public async Task<Player>? GetPlayerInformation(ulong playerID)
        {
            return await _entityClient.QuerySingleAsync<Player>("SELECT * FROM btmoderation_players WHERE PlayerID = @0", playerID);
        }
        public async Task<int> GetPlayerWarningsCount(ulong playerID)
        {
            return await _entityClient.QuerySingleAsync<int>("SELECT Count(PlayerID) FROM moderation_playerwarns WHERE PlayerID = @0", playerID);
        }
        public async Task<int> GetPlayerNotationsCount(ulong playerID)
        {
            return await _entityClient.QuerySingleAsync<int>("SELECT COUNT(PlayerID) FROM btmoderation_notations WHERE PlayerID = @0", playerID);
        }
        public async Task<int> GetPlayerKicksCount(ulong playerID)
        {
            return await _entityClient.QuerySingleAsync<int>("SELECT COUNT(PlayerID) FROM btmoderation_kicks WHERE PlayerID = @0", playerID);
        }
        public async Task<int> GetPlayerBansCount(ulong playerID)
        {
            return await _entityClient.QuerySingleAsync<int>("SELECT * FROM moderation_playerbans WHERE PlayerID = @0", playerID);
        }
        public async Task<int> GetPlayerCommendationsCount(ulong playerID)
        {
            return await _entityClient.QuerySingleAsync<int>("SELECT * FROM btmoderation_commendations WHERE PlayerID = @0", playerID);
        }
        public async Task<List<Notation>> GetPlayerNotations(ulong playerID)
        {
            var notations = await _entityClient.QueryAsync<Notation>("SELECT * FROM btmoderation_notations WHERE PlayerID = @0", playerID);
            if (notations == null) return new List<Notation>();
            Console.WriteLine($"Database Notation Count: {notations.Count()}");
            return notations.ToList();
        }
        public async Task<List<Warning>> GetPlayerWarnings(ulong playerID)
        {
            var warnings = await _entityClient.QueryAsync<Warning>("SELECT * FROM moderation_playerwarns WHERE PlayerID = @0", playerID);
            if (warnings == null) return new List<Warning>();
            Console.WriteLine($"Database Notation Count: {warnings.Count()}");
            return warnings.ToList();
        }
        public async Task<List<Kick>> GetPlayerKicks(ulong playerID)
        {
            var kicks = await _entityClient.QueryAsync<Kick>("SELECT * FROM btmoderation_kicks WHERE PlayerID = @0", playerID);
            if (kicks == null) return new List<Kick>();
            return kicks.ToList();
        }
        public async Task<List<Commendation>> GetPlayerCommendations(ulong playerID)
        {
            var commendations = await _entityClient.QueryAsync<Commendation>("SELECT * FROM btmoderation_kicks WHERE PlayerID = @0", playerID);
            if (commendations == null) return new List<Commendation>();
            return commendations.ToList();
        }
        public async Task<List<Ban>> GetPlayerBans(ulong playerID)
        {
            var bans = await _entityClient.QueryAsync<Ban>("SELECT * FROM moderation_playerbans WHERE PlayerID = @0", playerID);
            if (bans == null) return new List<Ban>();
            return bans.ToList();
        }
        public async Task<List<UsernameChanges>> GetPlayerRecentUsernames(ulong playerID)
        {
            var recentNames = await _entityClient.QueryAsync<UsernameChanges>("SELECT * FROM btmoderation_usernamechanges WHERE PlayerID = @0", playerID);
            if (recentNames == null) return new List<UsernameChanges>();
            return recentNames.ToList();
        }
        public async Task<Links?> GetPlayerLinkedAccount(string code)
        {
            var linkedAccount = await _entityClient.QuerySingleAsync<Links?>("SELECT * FROM btlinks_links WHERE Code = @0", code);
            if (linkedAccount == null) return null;
            return linkedAccount;
        }
        public async Task<Links?> GetPlayerLinkedAccount(ulong discordID)
        {
            var linkedAccount = await _entityClient.QuerySingleAsync<Links?>("SELECT * FROM btlinks_links WHERE DiscordID = @0", discordID);
            if (linkedAccount == null) return null;
            return linkedAccount;
        }
        public async Task LinkAccount(ulong playerID, ulong DiscordID)
        {
            await _entityClient.ExecuteNonQueryAsync("UPDATE btlinks_links SET Linked = @0, DiscordID = @1, LinkedDate = @2 WHERE PlayerID = @3", true, DiscordID, DateTime.Now, playerID);
        }
    }
}

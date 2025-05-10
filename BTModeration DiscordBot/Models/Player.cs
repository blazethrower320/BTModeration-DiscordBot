using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTModeration_DiscordBot.Models
{
    public class Player
    {
        public int ID { get; set; }
        public ulong PlayerID { get; set; }
        public string Username { get; set; }
        public string ProfileImage { get; set; }
        public string SteamLink { get; set; }
        public int PlayTime { get; set; }
        public int Trustscore { get; set; }
        public DateTime FirstPlayed { get; set; }
        public DateTime LastPlayed { get; set; }
    }
}

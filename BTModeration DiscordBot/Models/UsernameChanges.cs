using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTModeration_DiscordBot.Models
{
    public class UsernameChanges
    {
        public int ID { get; set; }
        public ulong PlayerID { get; set; }
        public string OldUsername { get; set; }
        public string NewUsername { get; set; }
        public DateTime Date { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTModeration_DiscordBot.Models
{
    public class Links
    {
        public int Id { get; set; }
        public ulong PlayerID { get; set; }
        public string Code { get; set; }
        public bool Linked { get; set; }
        public ulong DiscordID { get; set; }
        public DateTime LinkedDate { get; set; }
    }
}

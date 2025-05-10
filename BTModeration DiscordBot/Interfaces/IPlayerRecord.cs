using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTModeration_DiscordBot.Interfaces
{
    public interface IPlayerRecord
    {
        public int ID { get; set; }
        public ulong PlayerID { get; set; }
        public ulong ModeratorID { get; set; }
        public string Reason { get; set; }
        public DateTime Issued { get; set; }
    }
}

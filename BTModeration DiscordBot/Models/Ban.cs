﻿using BTModeration_DiscordBot.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTModeration_DiscordBot.Models
{
    public class Ban
    {
        public int ID { get; set; }
        public ulong PlayerID { get; set; }
        public ulong ModeratorID { get; set; }
        public string Reason { get; set; }
        public long TimeBanned { get; set; }
        public int BanLength { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Koneko.Bot.Db
{
    public class RankReward
    {
        public int Id { get; set; }
        public UInt64 RoleId { get; set; }
        public UInt64 GuildId { get; set; }
        public UInt64 ReqScore { get; set; }
    }
}

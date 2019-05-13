using System;
using System.Collections.Generic;
using System.Text;

namespace Koneko.Bot.Db
{
    public class UserStatistic
    {
        public int Id { get; set; }
        public UInt64 UserId { get; set; }
        public UInt64 GuildId { get; set; }
        public UInt64 Score { get; set; }
        public DateTime LastScoredMessage { get; set; }
    }
}

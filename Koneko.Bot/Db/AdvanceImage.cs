using System;
using System.Collections.Generic;
using System.Text;

namespace Koneko.Bot.Db
{
    public class AdvanceImage
    {
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public string Url { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Koneko.Bot.Db
{
    class BotResponse
    {
        public int Id { get; set; }
        public ulong ResponseId { get; set; }
        public ulong MessageId { get; set; }
    }
}

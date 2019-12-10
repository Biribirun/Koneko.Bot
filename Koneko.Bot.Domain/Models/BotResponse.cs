using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Koneko.Bot.Domain.Models
{
    public class BotResponse
    {
        [Key]
        public ulong ResponseId { get; set; }
        public ulong GuildId { get; set; }
        public ulong MessageId { get; set; }
    }
}

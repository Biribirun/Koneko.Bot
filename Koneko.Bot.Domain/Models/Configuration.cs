using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Koneko.Bot.Domain.Models
{
    public class Configuration
    {
        public ulong GuildId { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
}

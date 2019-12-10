using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Koneko.Bot.Domain.Models
{
    public class AdvanceImage
    {
        [Key]
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public string Url { get; set; }
    }
}

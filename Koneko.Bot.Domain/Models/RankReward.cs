using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Koneko.Bot.Domain.Models
{
    public class RankReward
    {
        [Key]
        public int Id { get; set; }
        public UInt64 GuildId { get; set; }
        public UInt64 RoleId { get; set; }
        public UInt64 ReqScore { get; set; }
        public UInt64 AddedBy { get; set; }
        public DateTime Modified { get; set; }
    }
}

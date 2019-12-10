using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Koneko.Bot.Domain.Models
{
    public class UserStatistic
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }
        public UInt64 UserId { get; set; }
        public UInt64 GuildId { get; set; }
        public UInt64 Score { get; set; }
        public DateTime LastScoredMessage { get; set; }
    }
}

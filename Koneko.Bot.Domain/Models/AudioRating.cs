using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Koneko.Bot.Domain.Models
{
    public enum LikeType
    {       
       None,
       Like,
       Dislike
    }
    public class AudioRating
    {
        public AudioRating()
        {

        }
        public AudioRating(AudioRating audioRating)
        {
            GuildId = audioRating.GuildId;
            AudioIdentifier = audioRating.AudioIdentifier;
            UserId = audioRating.UserId;
            LikeType = audioRating.LikeType;            
        }

        public ulong GuildId { get; set; }
        public string AudioIdentifier { get; set; }
        public ulong UserId { get; set; }

        public LikeType LikeType { get; set; }
    }
}

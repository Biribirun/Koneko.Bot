using Koneko.Bot.DataAccessLayer.Data;
using Koneko.Bot.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;


namespace Koneko.Bot.DataAccessLayer.Repositories
{
    public class AudioRatingRepository : IRepository
    {
        private readonly KonekoContext _konekoContext;
        private readonly MemoryCache _memoryCache;
        public AudioRatingRepository(KonekoContext konekoContext, MemoryCache memoryCache)
        {
            _konekoContext = konekoContext;
            _memoryCache = memoryCache;
        }

        public async Task<List<AudioRating>> GetAudioRatingForUser(ulong userId)
        {
            return await (from i in _konekoContext.AudioRatings.AsNoTracking()
                          where i.UserId == userId
                          select i).ToListAsync();
        }

        public async Task<List<AudioRating>> GetAudioRatingForSong(ulong guildId, string audioIdentifier)
        {
            return await (from i in _konekoContext.AudioRatings.AsNoTracking()
                          where i.GuildId == guildId &&
                                i.AudioIdentifier == audioIdentifier
                          select i).ToListAsync();
        }

        public async Task<AudioRating> SetAudioRating(AudioRating audioRating)
        {
            if (audioRating is null)
                return null;

            await _konekoContext.AudioRatings
                .Upsert(audioRating)
                .On(k => new { k.AudioIdentifier, k.GuildId, k.UserId })
                .WhenMatched(k => new AudioRating
                {
                    LikeType = k.LikeType == audioRating.LikeType ? LikeType.None : audioRating.LikeType
                }).RunAsync();

            await _konekoContext.SaveChangesAsync();

            return audioRating;
        }


    }
}

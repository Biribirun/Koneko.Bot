using Koneko.Bot.DataAccessLayer.Data;
using Koneko.Bot.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Koneko.Bot.DataAccessLayer.Repositories
{
    public class AdvanceImagesRepository : IRepository
    {
        private readonly KonekoContext _konekoContext;
        private readonly MemoryCache _memoryCache;

        public AdvanceImagesRepository(KonekoContext konekoContext, MemoryCache memoryCache)
        {
            _konekoContext = konekoContext;
            _memoryCache = memoryCache;
        }

        public async Task<List<AdvanceImage>> GetAdvancesImage(ulong GuildId)
        {
            return await (from i in _konekoContext.AdvanceImages.AsNoTracking()
                          where i.GuildId == GuildId
                          select i).ToListAsync();           
        }

        public async Task<AdvanceImage> AddAdvanceImage(AdvanceImage advanceImage)
        {
            if (advanceImage is null)
                return null;

            await _konekoContext.AdvanceImages.AddAsync(advanceImage);
            await _konekoContext.SaveChangesAsync();

            return advanceImage;
        }

        public async Task<bool> RemoveAdvanceImage(AdvanceImage advanceImage)
        {
            if (advanceImage is null)
                return false;

            _konekoContext.Entry(advanceImage).State = EntityState.Modified;
            _konekoContext.AdvanceImages.Remove(advanceImage);
            await _konekoContext.SaveChangesAsync();

            return true;
        }
    }
}

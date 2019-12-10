using Koneko.Bot.DataAccessLayer.Data;
using Koneko.Bot.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace Koneko.Bot.DataAccessLayer.Repositories
{
    public class ConfigurationRepository
    {
        private readonly KonekoContext _konekoContext;
        private readonly MemoryCache _memoryCache;

        public ConfigurationRepository(KonekoContext konekoContext, MemoryCache memoryCache)
        {
            _konekoContext = konekoContext;
            _memoryCache = memoryCache;
        }

        object GetKey(ulong guildId, string key)
            => new { guildId, key };
        public Task<Configuration> GetConfiguration(ulong guildId, string key)
        {
            return _memoryCache.GetOrCreateAsync(GetKey(guildId, key), x => 
             (from i in _konekoContext.Configurations.AsNoTracking()
                          where i.GuildId == guildId && i.Key == key
                          select i).FirstOrDefaultAsync());
        }

        public async Task<T> GetConfiguration<T>(ulong guildId, string key)
        {
            var conf = await GetConfiguration(guildId, key);
            return JsonConvert.DeserializeObject<T>(conf.Value);
        }

        public async Task<Configuration> SetConfiguration(Configuration configuration)
        {
            await _konekoContext
                .Upsert(configuration)
                .On(k => new { k.Key, k.GuildId })
                .WhenMatched(k => new Configuration
                {
                    Value = configuration.Value
                }).RunAsync();

            _memoryCache.Remove(GetKey(configuration.GuildId, configuration.Key));            
            await _konekoContext.SaveChangesAsync();

            return configuration;
        }
    }
}

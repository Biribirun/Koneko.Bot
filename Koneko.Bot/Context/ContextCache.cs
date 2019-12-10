using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Koneko.Bot.Context
{
    public class ContextCache
    {
        private readonly ConcurrentDictionary<ulong, GuildContext> _contexts;

        public ContextCache()
        {
            _contexts = new ConcurrentDictionary<ulong, GuildContext>();
        }

        public GuildContext GetGuildContext(ulong guildContext)
        {
            return _contexts.GetOrAdd(guildContext, x => new GuildContext(x));
        }

    }
}

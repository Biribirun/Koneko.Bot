using Discord;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Discord.WebSocket;
using Koneko.Bot.Db;

namespace Koneko.Bot
{
    public class ResponseRemover
    {
        private DbConnection _db;
        public ResponseRemover(DbConnection dbConnection)
        {
            _db = dbConnection;
        }
        public async Task CheckAndRemove(Cacheable<IMessage, ulong> message, ISocketMessageChannel channel)
        {
            var responses = _db.Repository.Query<Db.BotResponse>().Where(x => x.MessageId == message.Id).ToEnumerable();

            foreach(var i in responses)
            {
                await channel.DeleteMessageAsync(i.ResponseId);
            }
        }
    }
}

using Koneko.Bot.DataAccessLayer.Data;
using Koneko.Bot.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Koneko.Bot.DataAccessLayer.Repositories
{
    public class BotResponsesRepository : IRepository
    {
        private readonly KonekoContext _konekoContext;

        public BotResponsesRepository(KonekoContext konekoContext)
        {
            _konekoContext = konekoContext;
        }

        public async Task<BotResponse> AddBotResponse(BotResponse botResponse)
        {
            await _konekoContext.BotResponses.AddAsync(botResponse);
            await _konekoContext.SaveChangesAsync();

            return botResponse;
        }

        public async Task<List<BotResponse>> GetBotResponses(ulong guildId, ulong messageId)
        {
            return await (from i in _konekoContext.BotResponses
                          where i.GuildId == guildId && i.MessageId == messageId
                          select i).ToListAsync();
        }

        public async Task<bool> RemoveBotResponse(BotResponse botResponse)
        {
            if(botResponse is null)
            {
                return false;
            }

            _konekoContext.Remove(botResponse);
            await _konekoContext.SaveChangesAsync();
            return true;
        }
    }
}

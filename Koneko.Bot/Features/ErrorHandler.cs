using Discord;
using Discord.Commands;
using Koneko.Bot.Db;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Koneko.Bot
{
    public class ErrorHandler
    {
        private DbConnection _db;
        
        public ErrorHandler(DbConnection repository)
        {
            _db = repository;
        }

        public async Task<IUserMessage> SendErrorAsync(CommandContext context, IResult result)
        {
            var response = await context.Channel.SendMessageAsync(result.ErrorReason);

            _db.Repository.Insert(new Db.BotResponse
            {
                MessageId = context.Message.Id,
                ResponseId = response.Id
            });

            return response;
        }
    }
}

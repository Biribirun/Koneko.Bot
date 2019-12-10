using Discord;
using Discord.Commands;
using Koneko.Bot.DataAccessLayer.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Koneko.Bot.Services
{
    public class ErrorHandlerService
    {
        private readonly MessageRemoverService _responseRemover;

        public ErrorHandlerService(MessageRemoverService responseRemover)
        {
            _responseRemover = responseRemover;
        }

        public async Task<IUserMessage> SendErrorAsync(CommandContext context, IResult result)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var response = await context.Channel.SendMessageAsync(result?.ErrorReason);

            await _responseRemover.SaveBotResponse(context, response);
            
            return response;
        }
    }
}

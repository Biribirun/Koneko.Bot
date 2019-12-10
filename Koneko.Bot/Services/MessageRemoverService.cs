using Discord;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Discord.WebSocket;
using Discord.Commands;
using Koneko.Bot.DataAccessLayer.Repositories;
using Koneko.Bot.Domain.Models;
using Newtonsoft.Json;
using static Koneko.Bot.Services.PlayerService;

namespace Koneko.Bot
{
    public class MessageRemoverService
    {
        private readonly BotResponsesRepository _botResponsesRepository;
        private readonly ConfigurationRepository _configurationRepository;
        public MessageRemoverService(BotResponsesRepository botResponsesRepository, ConfigurationRepository configurationRepository)
        {
            _botResponsesRepository = botResponsesRepository;
            _configurationRepository = configurationRepository;
        }

        public async Task DeletedCheckAndRemove(Cacheable<IMessage, ulong> message, SocketTextChannel channel)
        {
            if (channel is null)
            {
                throw new ArgumentNullException(nameof(channel));
            }

            var guildId = channel.Guild.Id;

            var responses = await _botResponsesRepository.GetBotResponses(guildId, message.Id);
            foreach(var i in responses)
            {
                await channel.DeleteMessageAsync(i.ResponseId);
            }
        }

        public async Task DeleteMessageFromBotOnlyChannels(CommandContext context)
        {
            if (context.User.IsBot)
                return;
            var guildId = context.Guild.Id;

            var conf = await _configurationRepository.GetConfiguration(guildId, "PlayerViewConfiguration");

            var playerConf = JsonConvert.DeserializeObject<PlayerConfiguration>(conf.Value);

            var playerChat = playerConf?.TextChatId;

            var channel = context.Channel;
            if(playerChat == channel.Id)
            {
                await channel.DeleteMessageAsync(context.Message);
            }

        }
        public async Task SaveBotResponse(ICommandContext context, IUserMessage response)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (response is null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            var botResponse = new BotResponse
            {
                GuildId = context.Guild.Id,
                MessageId = context.Message.Id,
                ResponseId = response.Id
            };

            await _botResponsesRepository.AddBotResponse(botResponse);
        }
    }
}

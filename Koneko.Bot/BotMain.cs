using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Koneko.Bot.DataAccessLayer.Repositories;
using Koneko.Bot.Services;
using Lavalink4NET;
using Lavalink4NET.Tracking;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Koneko.Bot
{
    public class BotMain : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly CommandService _commands;
        private readonly MessageRemoverService _responseRemover;
        private readonly ErrorHandlerService _errorHandler;
        private readonly Statistics _statistics;
        private readonly IAudioService _audioService;
        private readonly DiscordSocketClient _client;
        private readonly PlayerService _playerService;
        private readonly ConfigurationRepository _configurationRepository;
        private readonly InactivityTrackingService _inactivityTrackingService;

        public BotMain(IServiceProvider services, MessageRemoverService responseRemover, ErrorHandlerService errorHandler, Statistics statistics, IConfiguration configuration, IAudioService audioService, DiscordSocketClient client, PlayerService playerService, ConfigurationRepository configurationRepository, InactivityTrackingService inactivityTrackingService)
        {
            _configuration = configuration;
            _responseRemover = responseRemover;
            _errorHandler = errorHandler;
            _statistics = statistics;
            _serviceProvider = services;
            _audioService = audioService;
            _client = client;
            _playerService = playerService;
            _configurationRepository = configurationRepository;
            _inactivityTrackingService = inactivityTrackingService;

            _commands = new CommandService();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _commands.Log += _commands_Log;

            _client.Log += Log;
            _client.Ready += _client_Ready;
            _client.UserJoined += Client_UserJoined;
            _client.Connected += Client_Connected;
            _client.MessageDeleted += Client_MessageDeleted;
            _client.UserVoiceStateUpdated += Client_UserVoiceStateUpdated;
            _client.ReactionAdded += Client_ReactionAdded;
            
                await InstallCommands();

            string token = string.Empty;
            bool prod = false;
            prod = bool.Parse(_configuration["prod"]);
            if (prod)
                token = _configuration["tokenProd"];
            else
                token = _configuration["tokenTest"];

            Console.WriteLine($"Production: {prod}");

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private async Task _client_Ready()
        {
            await _audioService.InitializeAsync();
            //_inactivityTrackingService.BeginTracking();
        }

        private async Task Client_ReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            if (await _playerService.HandleReaction(arg1, arg2, arg3))
                return;
        }

        private Task Client_UserVoiceStateUpdated(SocketUser arg1, SocketVoiceState arg2, SocketVoiceState arg3)
        {
            return Task.CompletedTask;
            //throw new NotImplementedException();
        }

        public async Task InstallCommands()
        {
            // Hook the MessageReceived Event into our Command Handler
            _client.MessageReceived += MessageReceived;
            // Discover all of the commands in this assembly and load them.
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);
        }

        private async Task _commands_Log(LogMessage arg)
        {
            Console.WriteLine(arg);
            await Task.CompletedTask;
        }
        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private Task Client_Connected()
        {
            Console.WriteLine(_client.CurrentUser.Username);
            return Task.CompletedTask;
        }

        private async Task Client_MessageDeleted(Cacheable<IMessage, ulong> messageChache, ISocketMessageChannel messageChannel)
        {
            if (messageChannel is null)
            {
                throw new ArgumentNullException(nameof(messageChannel));
            }

            var socketTextChannel = messageChannel as SocketTextChannel;

            await _responseRemover.DeletedCheckAndRemove(messageChache, socketTextChannel);
        }
        private async Task Client_UserJoined(SocketGuildUser arg)
        {
            var role = arg.Guild.Roles.Where(i => i.Name.ToLower().Equals("obserwatorzy")).FirstOrDefault();
            if (role is null)
                return;
            await arg.AddRoleAsync(role);
        }

        private async Task MessageReceived(SocketMessage messageParam)
        {
            if (messageParam as SocketUserMessage == null) return;
            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;
            // Determine if the message is a command, based on if it starts with '!' or a mention prefix
            // Create a Command Context
            var context = new CommandContext(_client, messageParam as SocketUserMessage);

            if ((messageParam as SocketUserMessage).HasCharPrefix('!', ref argPos) || (messageParam as SocketUserMessage).HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                // Execute the command. (result does not indicate a return value, 
                // rather an object stating if the command executed successfully)
                var input = context.Message.Content.Substring(argPos);

                var command = _commands.Search(input);

                bool exec = true;

                if (command.Commands.Any(x => x.Command.Module.Name == "MiscCommands"))
                {
                    var conf = await _configurationRepository.GetConfiguration(context.Guild.Id, "MiscCommandsEnabled");
                    if (conf != null)
                    {
                        var enabled = JsonConvert.DeserializeObject<bool>(conf?.Value);
                        if(!enabled)
                        {
                            exec = false;
                        }
                    }

                    if(conf is null)
                    {
                        exec = false;
                    }
                }

                if(exec)
                {
                    var result = await _commands.ExecuteAsync(context, input, _serviceProvider);
                    if (!result.IsSuccess)
                    {
                        var response = await _errorHandler.SendErrorAsync(context, result);
                    }
                }
            }
            else
            {
                await _statistics.AddPoints(context);
            }

            await _responseRemover.DeleteMessageFromBotOnlyChannels(context);
        }
    }
}

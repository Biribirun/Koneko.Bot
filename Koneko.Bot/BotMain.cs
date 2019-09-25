using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Koneko.Bot
{
    class BotMain
    {
        private DiscordSocketClient client;
        private IServiceProvider Services;
        private CommandService _commands;
        private ResponseRemover ResponseRemover;
        private ErrorHandler ErrorHandler;
        private Statistics Statistics;

        public BotMain(IServiceProvider services, ResponseRemover responseRemover, ErrorHandler errorHandler, Statistics statistics)
        {
            ResponseRemover = responseRemover;
            ErrorHandler = errorHandler;
            Statistics = statistics;
            Services = services;
        }

        public async Task<int> Main(string[] args)
        {
            client = new DiscordSocketClient();
            _commands = new CommandService();
            _commands.Log += _commands_Log;

            client.Log += Log;
            client.UserJoined += Client_UserJoined;
            client.Connected += Client_Connected;
            client.MessageDeleted += Client_MessageDeleted;
            
            await InstallCommands();

            string token;
            var prod = bool.Parse(ConfigurationManager.AppSettings["prod"]);
            if (prod)
                token = ConfigurationManager.AppSettings["tokenTest"];
            else
                token = ConfigurationManager.AppSettings["tokenProd"];

            Console.WriteLine($"Production: {prod}");

            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            await Task.Delay(-1);
            return 0;
        }

        public async Task InstallCommands()
        {
            // Hook the MessageReceived Event into our Command Handler
            client.MessageReceived += MessageReceived;
            // Discover all of the commands in this assembly and load them.
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), Services);
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
            Console.WriteLine(client.CurrentUser.Username);
            return Task.CompletedTask;
        }

        private async Task Client_MessageDeleted(Cacheable<IMessage, ulong> arg1, ISocketMessageChannel arg2)
        {
            await ResponseRemover.CheckAndRemove(arg1, arg2);
        }
        private async Task Client_UserJoined(SocketGuildUser arg)
        {
            var role = arg.Guild.Roles.Where(i => i.Name.ToLower().Equals("obserwatorzy")).FirstOrDefault();
            if ((role is null))
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
            var context = new CommandContext(client, messageParam as SocketUserMessage);

            if (!((messageParam as SocketUserMessage).HasCharPrefix('!', ref argPos) || (messageParam as SocketUserMessage).HasMentionPrefix(client.CurrentUser, ref argPos)))
            {
                Statistics.AddPoints(context);
                return;
            }

            // Execute the command. (result does not indicate a return value, 
            // rather an object stating if the command executed successfully)
            var result = await _commands.ExecuteAsync(context, argPos, Services);
            if (!result.IsSuccess)
            {
                await ErrorHandler.SendErrorAsync(context, result);
            }
        }
    }
}

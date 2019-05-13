using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Configuration;
using System.Collections.Generic;

namespace Koneko.Bot
{
    class Program
    {
        public static IEnumerable<CommandInfo> Commands => _commands.Commands;

        private IServiceProvider services;
        private static CommandService _commands;
        private DiscordSocketClient client;

        static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        public async Task MainAsync()
        {
            client = new DiscordSocketClient();
            _commands = new CommandService();
            _commands.Log += _commands_Log;

            services = new ServiceCollection()
                .BuildServiceProvider();

            await InstallCommands();


            client.Log += Log;
            client.UserJoined += Client_UserJoined;
            client.Connected += Client_Connected;
            
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
        }

        private Task Client_Connected()
        {
            Console.WriteLine(client.CurrentUser.Username);
            return Task.CompletedTask;
        }

        private async Task _commands_Log(LogMessage arg)
        {
            Console.WriteLine(arg);
            await Task.CompletedTask;
        }

        private async Task Client_UserJoined(SocketGuildUser arg)
        {
            var role = arg.Guild.Roles.Where(i => i.Name.ToLower().Equals("obserwatorzy")).FirstOrDefault();
            await arg.AddRoleAsync(role);
        }

        public async Task InstallCommands()
        {
            // Hook the MessageReceived Event into our Command Handler
            client.MessageReceived += MessageReceived;
            // Discover all of the commands in this assembly and load them.
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);
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
                Statistics.Statistics.AddPoints(context);
                return;
            }

            // Execute the command. (result does not indicate a return value, 
            // rather an object stating if the command executed successfully)
            var result = await _commands.ExecuteAsync(context, argPos, services);
            if (!result.IsSuccess)
                await context.Channel.SendMessageAsync(result.ErrorReason);
        }
    }
}

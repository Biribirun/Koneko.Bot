using Discord.Commands;
using Koneko.Bot.ModuleBaseExtension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koneko.Bot.Commands
{
    public class HelpCommand : ModuleBaseEx
    {
        private readonly CommandService _commands;
        public HelpCommand(MessageRemoverService responseRemover, CommandService commands)
            : base(responseRemover)
        {
            _commands = commands;
        }

        [Command("help"), Summary("Wyświetla pomoc.")]
        public async Task Help()
        {
            var sb = new StringBuilder();

            foreach (var command in _commands.Commands)
            {
                var preconditionResult = await command.CheckPreconditionsAsync(Context);

                if (!preconditionResult.IsSuccess)
                    continue;

                sb.Append($"!{command.Name} - {command.Summary}");

                IEnumerable<string> argumentsinfo = command.Parameters.Select(
                    x => "[" +
                    (x.IsOptional ? "[" : "") +
                    $"[{x.Name}] <{x.Type.Name}>]" +
                    (x.IsOptional ? "]" : ""));

                sb.Append($" {string.Join("\t", argumentsinfo)}\n");
            }

            await ReplyImage(description: sb.ToString());
        }
    }
}

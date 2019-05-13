using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Koneko.Bot.ModuleBaseExtension
{
    public class ModuleBaseEx : ModuleBase
    {
        public async Task ReplyImage(string description = null, string url = null)
        {
            var embed = new EmbedBuilder
            {
                ImageUrl = url,
                Description = description
            };

            await ReplyAsync(embed: embed.Build());
        }
    }
}

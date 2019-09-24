using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Koneko.Bot.Db;

namespace Koneko.Bot.ModuleBaseExtension
{
    public class ModuleBaseEx : ModuleBase
    {
        protected DbConnection _db;
        public ModuleBaseEx(DbConnection repository)
        {
            _db = repository;
        }

        public async Task ReplyImage(string description = null, string url = null)
        {
            var embed = new EmbedBuilder
            {
                ImageUrl = url,
                Description = description
            };

            var response = await ReplyAsync(embed: embed.Build());

            _db.Repository.Insert(new Db.BotResponse
            {
                MessageId = Context.Message.Id,
                ResponseId = response.Id
            });
        }
    }
}

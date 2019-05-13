using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using LiteDB;
using System.Threading.Tasks;
using Koneko.Bot.ModuleBaseExtension;

namespace Koneko.Bot.Statistics
{
    public class Commands : ModuleBaseEx
    {
        [Command("top"), Summary("Wyświetla listę rankingową.")]
        public async Task Top(int page = 1)
        {
            page--;
            using (var db = new LiteDB.LiteRepository("Koneko.db"))
            {
                var users = db.Query<Db.UserStatistic>().Where(x => x.GuildId == Context.Guild.Id)
                                                        .ToEnumerable()
                                                        .OrderByDescending(x => x.Score)
                                                        .Skip(10 * page)
                                                        .Take(10);

                var totalUsers = db.Query<Db.UserStatistic>().Where(x => x.GuildId == Context.Guild.Id).Count();

                var guildMembers = await Context.Guild.GetUsersAsync();

                var lines = from sUser in users
                            join gUser in guildMembers on sUser.UserId equals gUser.Id
                            select $"{gUser.Username} - {sUser.Score}";

                if(lines.Count() == 0)
                {
                    await ReplyImage(description: $"Brak użytkowników na pozycjach {page * 10} - {page * 10 + 10}");
                    return;
                }

                var sb = new StringBuilder();

                sb.Append(string.Join("\n", lines));
                sb.Append($"\n{page * 10} - {page * 10 + 10} / {totalUsers}");

                await ReplyImage(description: sb.ToString());
            }
        }
    }
}

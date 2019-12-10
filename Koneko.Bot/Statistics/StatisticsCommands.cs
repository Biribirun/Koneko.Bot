using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using LiteDB;
using System.Threading.Tasks;
using Koneko.Bot.ModuleBaseExtension;
using Koneko.Bot.Helpers;
using Koneko.Bot.DataAccessLayer.Repositories;
using Discord.WebSocket;

namespace Koneko.Bot
{
    public class StatisticsCommands : ModuleBaseEx
    {
        private readonly StatisticsRepository _statisticsRepository;
        public StatisticsCommands(MessageRemoverService responseRemover, StatisticsRepository statisticsRepository) : base(responseRemover)
        {
            _statisticsRepository = statisticsRepository;
        }

        [Command("top"), Summary("Wyświetla listę rankingową.")]
        public async Task Top(int page = 1)
        {
            page--;

            var currentUserStatistics = await _statisticsRepository.GetUserStatistics(Context.Guild.Id, Context.User.Id);

            var pagedUsers = await _statisticsRepository.GetStatistics(Context.Guild.Id, page);

            var totalUsers = (Context.Guild as SocketGuild).MemberCount;

            var guildMembers = await Context.Guild.GetUsersAsync();


            var formatFields = from sUser in pagedUsers
                               join gUser in guildMembers on sUser.UserId equals gUser.Id into users
                               from gUser in users.DefaultIfEmpty()
                               select new string[2] { gUser == null ? $"Użytkownik opuścił serwer ID: {sUser.UserId}" : Formatters.GetUserName(gUser), sUser.Score.ToString() };

            if(!formatFields.Any())
            {
                await ReplyImage(description: $"Brak użytkowników na pozycjach {page * 10 + 1} - {page * 10 + 10}");
                return;
            }

            int rownum = 1;

            var lines = from field in formatFields
                            select string.Format("[{0}]", (page * 10) + rownum++).PadRight(12) + "> " +
                                $"#{field[0]}\n" +
                                $"Całkowity wynik:".PadLeft(32) + $" {field[1]}";

            var sb = new StringBuilder();

            sb.Append($"Ranking użytkowników serwera {Context.Guild.Name}\n\n");
            sb.Append(string.Join("\n", lines));
            sb.Append('\n');
            sb.Append(string.Concat(Enumerable.Repeat('-', 40)));
            //sb.Append($"\nTwoje miejsce w rankingu: {guildUsers.TakeWhile(x => x.UserId != Context.User.Id).Count() + 1} / {totalUsers}\n");
            sb.Append($"Całkowity wynik: {currentUserStatistics.Score}");
            await ReplyImage(description: $"```CS\n{sb.ToString()}```");
        }
    }
}

using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Discord.WebSocket;
using System.Threading.Tasks;
using Discord;

namespace Koneko.Bot.Statistics
{
    public class Statistics
    {
        private static Random _rand = new Random();

        public static void AddPoints(CommandContext context)
        {
            if (context.User.IsBot)
                return;
            var GuildId = context.Guild.Id;
            var UserId = context.User.Id;
            ulong Points = (ulong)_rand.Next(10, 20);

            using (var db = new LiteDB.LiteRepository("Koneko.db"))
            {
                var statistics = db.Query<Db.UserStatistic>().Where(x => x.GuildId == GuildId && x.UserId == UserId).FirstOrDefault();
                if (statistics is null)
                {
                    statistics = new Db.UserStatistic()
                    {
                        UserId = UserId,
                        GuildId = GuildId,
                        Score = Points,
                        LastScoredMessage = DateTime.Now
                    };
                    db.Insert(statistics);
                }
                else
                {
                    if ((DateTime.Now - statistics.LastScoredMessage).Minutes > 2)
                    {
                        statistics.Score += Points;
                        statistics.LastScoredMessage = DateTime.Now;
                        db.Update(statistics);
                    }
                }

                var user = (context.User as SocketGuildUser);

                var roles = db.Query<Db.RankReward>().Where(x => x.GuildId == context.Guild.Id && statistics.Score >= x.ReqScore).ToList();

                var rolesToAdd =
                    from reward in roles
                    join userRole in context.Guild.Roles on reward.RoleId equals userRole.Id
                    where !user.Roles.Contains(userRole)
                    select userRole;

                user.AddRolesAsync(rolesToAdd);

                foreach(var i in rolesToAdd)
                {
                    var embed = new EmbedBuilder
                    {
                        Description = $"{user.Username} staje się {i.Name}!"
                    };
                    context.Channel.SendMessageAsync(embed: embed.Build());
                }
            }

        }
    }
}

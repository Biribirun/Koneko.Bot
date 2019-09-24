using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using LiteDB;
using Discord.WebSocket;
using System.Threading.Tasks;
using Discord;
using Koneko.Bot.Helpers;
using Koneko.Bot.Db;

namespace Koneko.Bot
{
    public class Statistics
    {
        private DbConnection _db;
        public Statistics(DbConnection db)
        {
            _db = db;
        }

        private Random _rand = new Random();

        public void AddPoints(CommandContext context)
        {
            if (context.User.IsBot)
                return;
            var GuildId = context.Guild.Id;
            var UserId = context.User.Id;
            ulong Points = (ulong)_rand.Next(10, 20);
            
            var statistics = _db.Repository.Query<Db.UserStatistic>().Where(x => x.GuildId == GuildId && x.UserId == UserId).FirstOrDefault();
            if (statistics is null)
            {
                statistics = new Db.UserStatistic()
                {
                    UserId = UserId,
                    GuildId = GuildId,
                    Score = Points,
                    LastScoredMessage = DateTime.Now
                };
                _db.Repository.Insert(statistics);
            }
            else
            {
                if ((DateTime.Now - statistics.LastScoredMessage).Minutes < 2)
                {
                    return;
                }
                statistics.Score += Points;
                statistics.LastScoredMessage = DateTime.Now;
                _db.Repository.Update(statistics);
            }

            var user = (context.User as SocketGuildUser);

            var roles = _db.Repository.Query<Db.RankReward>().Where(x => x.GuildId == context.Guild.Id && statistics.Score >= x.ReqScore).ToEnumerable();

            var userRoles =(
                from reward in roles
                join userRole in context.Guild.Roles on reward.RoleId equals userRole.Id
                orderby reward.ReqScore
                select userRole).ToArray();

            if(userRoles.Length <= 0)
            {
                return;
            }

            IEnumerable<IRole> rolesToRemove = userRoles[0..^1];
            var roleToAdd = userRoles[^1];

            user.RemoveRolesAsync(rolesToRemove);

            if (!user.Roles.Contains(roleToAdd))
            {
                user.AddRoleAsync(roleToAdd);

                var image = _db.Repository.Query<AdvanceImage>().Where(x => x.GuildId == context.Guild.Id).ToEnumerable().RandomElement(new Random());

                var embed = new EmbedBuilder
                {
                    Description = $"{Formatters.GetUserName(user)} awansuje na {roleToAdd.Name}",
                    ImageUrl = image?.Url
                };
                context.Channel.SendMessageAsync(embed: embed.Build());
            }
        }

    }
}

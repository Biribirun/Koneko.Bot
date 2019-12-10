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
using Koneko.Bot.DataAccessLayer.Repositories;
using Koneko.Bot.Domain.Models;

namespace Koneko.Bot
{
    public class Statistics
    {
        private readonly StatisticsRepository _statisticsRepository;
        private readonly AdvanceImagesRepository _advanceImagesRepository;

        public Statistics(StatisticsRepository statisticsRepository, AdvanceImagesRepository advanceImagesRepository)
        {
            _statisticsRepository = statisticsRepository;
            _advanceImagesRepository = advanceImagesRepository;
        }

        private Random _rand = new Random();

        public async Task AddPoints(CommandContext context)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            if (context.User.IsBot)
                return;

            var GuildId = context.Guild.Id;
            var UserId = context.User.Id;
            ulong Points = (ulong)_rand.Next(10, 20);

            var statistics = await _statisticsRepository.GetUserStatistics(GuildId, UserId);
            if (statistics is null)
            {
                statistics = new UserStatistic()
                {
                    UserId = UserId,
                    GuildId = GuildId,
                    Score = Points,
                    LastScoredMessage = DateTime.Now
                };
                await _statisticsRepository.AddUserStatistic(statistics);
            }
            else
            {
                if ((DateTime.Now - statistics.LastScoredMessage).Minutes < 2)
                {
                    return;
                }
                statistics.Score += Points;
                await _statisticsRepository.UpdateStatistics(statistics);
            }

            var user = (context.User as SocketGuildUser);

            var roles = await _statisticsRepository.GetRankRewards(GuildId);

            var userRoles =(
                from reward in roles
                join userRole in context.Guild.Roles on reward.RoleId equals userRole.Id
                where reward.ReqScore <= statistics.Score
                orderby reward.ReqScore
                select userRole).ToArray();

            if(userRoles.Length <= 0)
            {
                return;
            }

            IEnumerable<IRole> rolesToRemove = userRoles[0..^1];
            var roleToAdd = userRoles[^1];

            await user.RemoveRolesAsync(rolesToRemove);

            if (!user.Roles.Contains(roleToAdd))
            {
                await user.AddRoleAsync(roleToAdd);

                var image = (await _advanceImagesRepository.GetAdvancesImage(GuildId)).RandomElement(_rand);

                var embed = new EmbedBuilder
                {
                    Description = $"{Formatters.GetUserName(user)} awansuje na {roleToAdd.Name}",
                    ImageUrl = image?.Url
                };
                await context.Channel.SendMessageAsync(embed: embed.Build());
            }
        }
    }
}

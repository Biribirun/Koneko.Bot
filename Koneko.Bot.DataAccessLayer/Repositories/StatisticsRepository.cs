using Koneko.Bot.DataAccessLayer.Data;
using Koneko.Bot.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;

namespace Koneko.Bot.DataAccessLayer.Repositories
{
    public class StatisticsRepository : IRepository
    {
        private readonly KonekoContext _konekoContext;

        public StatisticsRepository(KonekoContext konekoContext, MemoryCache memoryCache)
        {
            _konekoContext = konekoContext;
        }

        public Task<List<RankReward>> GetRankRewards(ulong guildId)
        {
            var rankRewards =
                from i in _konekoContext.RankRewards.AsNoTracking()
                where i.GuildId == guildId
                orderby i.ReqScore
                select i;

            return rankRewards.ToListAsync();
        }

        public async Task<RankReward> GetRankReward(ulong guildId, ulong roleId)
        {
            return await (from i in _konekoContext.RankRewards
                 where i.GuildId == guildId && i.RoleId == roleId
                 orderby i.ReqScore
                 select i).FirstOrDefaultAsync();
        }

        public async Task<RankReward> AddRankReward(RankReward rankReward)
        {
            _konekoContext.RankRewards.Add(rankReward);
            await _konekoContext.SaveChangesAsync();

            return rankReward;
        }

        public async Task<bool> UpdateRankReward(RankReward rankReward)
        {
            bool changed = false;

            if(!(rankReward is null))
            {
                rankReward.Modified = DateTime.UtcNow;
                _konekoContext.Entry(rankReward).State = EntityState.Modified;
                await _konekoContext.SaveChangesAsync();
                changed = true;
            }
            return changed;
        }

        public async Task<bool> DeleteRankReward(RankReward rankReward)
        {
            bool deleted = false;

            if(!(rankReward is null))
            {
                _konekoContext.Remove(rankReward);
                await _konekoContext.SaveChangesAsync();
                deleted = true;
            }

            return deleted;
        }

        public Task<List<UserStatistic>> GetStatistics(ulong guildId, int page)
        {
            if(page < 0)
            {
                return null;
            }

            var statistics =
                (from s in _konekoContext.UserStatistics.AsNoTracking()
                 where s.GuildId == guildId
                 orderby s.Score descending
                 select s).Skip(page * 10).Take(10);

            return statistics.ToListAsync();
        }

        public async Task<UserStatistic> GetUserStatistics(ulong guildId, ulong userId)
        {
            return await (from s in _konekoContext.UserStatistics
                where s.GuildId == guildId && s.UserId == userId
                select s).FirstOrDefaultAsync();
        }

        public async Task<UserStatistic> UpdateStatistics(UserStatistic userStatistic)
        {
            if(userStatistic is null)
            {
                return await Task.FromResult<UserStatistic>(null);
            }

            userStatistic.LastScoredMessage = DateTime.UtcNow;
            _konekoContext.UserStatistics.Update(userStatistic);
            await _konekoContext.SaveChangesAsync();

            return userStatistic;
        }

        public async Task<UserStatistic> AddUserStatistic(UserStatistic userStatistic)
        {
            userStatistic.LastScoredMessage = DateTime.UtcNow;

            await _konekoContext.UserStatistics.AddAsync(userStatistic);
            await _konekoContext.SaveChangesAsync();

            return userStatistic;
        }
    }
}

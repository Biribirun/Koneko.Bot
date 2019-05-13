using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Koneko.Bot.ModuleBaseExtension;

namespace Koneko.Bot.Administration
{
    [RequireUserPermission(GuildPermission.Administrator, Group = "Permission")]
    [RequireOwner(Group = "Permission")]
    public class Commands : ModuleBaseEx
    {

        [Command("AddRewardRole"), Summary("Dodaje rangę do listy nagród.")]
        public async Task SetRewardRole(IRole role, ulong reqScore)
        {
            if(role is null)
            {
                await ReplyImage(description: "Musisz najpierw dodać role o takiej nazwie");
            }

            using (var db = new LiteDB.LiteRepository("Koneko.db"))
            {
                var rewardRole = db.Query<Db.RankReward>().Where(x => x.GuildId == Context.Guild.Id && x.RoleId == role.Id).FirstOrDefault();

                if (rewardRole is null)
                {
                    Db.RankReward rankReward = new Db.RankReward
                    {
                        GuildId = Context.Guild.Id,
                        ReqScore = reqScore,
                        RoleId = role.Id,
                    };
                    db.Insert(rankReward);
                }
                else
                {
                    if (rewardRole.ReqScore == 0)
                    {
                        db.Delete<Db.RankReward>(x => x.Id == rewardRole.Id);
                        await ReplyImage(description: $"Usunięto z listy nagród rangę {role.Name}.");
                    }
                    else
                    {
                        rewardRole.ReqScore = reqScore;
                        db.Update(rewardRole);
                        await ReplyImage(description: $"Do listy nagórd dodano rangę {role} za {reqScore} punktów");
                    }
                }
            }
        }

        [Command("GetRewardRoles"), Summary("Wyświetla listę nagród za punkty.")]
        public async Task GetRewardRoles()
        {
            using (var db = new LiteDB.LiteRepository("Koneko.db"))
            {
                var rewards = db.Query<Db.RankReward>().Where(x => x.GuildId == Context.Guild.Id).ToEnumerable();

                StringBuilder sb = new StringBuilder();

                foreach (var reward in rewards)
                {
                    var rolename = Context.Guild.Roles.Where(x => x.Id == reward.RoleId).FirstOrDefault().Name;
                    sb.Append($"{rolename} - {reward.ReqScore}\n");
                }

                await ReplyImage(description: sb.ToString());
            }
        }
    }
}

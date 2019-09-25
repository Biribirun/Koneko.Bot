using Discord;
using Discord.Commands;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Koneko.Bot.ModuleBaseExtension;
using Koneko.Bot.Db;

namespace Koneko.Bot.Administration
{
    [RequireUserPermission(GuildPermission.Administrator, Group = "Permission")]
    [RequireOwner(Group = "Permission")]
    public class Commands : ModuleBaseEx
    {
        public Commands(DbConnection repository) : base(repository)
        {

        }

        [Command("SetRewardRole"), Summary("Dodaje rangę do listy nagród.")]
        public async Task SetRewardRole(string roleName, ulong reqScore)
        {
            IRole role = Context.Guild.Roles.FirstOrDefault(x => x.Name == roleName);
            role ??= await Context.Guild.CreateRoleAsync(roleName);

            if (role is null)
            {
                await ReplyImage("Nie udało się dodać roli");
                return;
            }

            Db.RankReward rewardRole = _db.Repository.Query<Db.RankReward>().Where(x => x.GuildId == Context.Guild.Id && x.RoleId == role.Id).FirstOrDefault();

            if (rewardRole is null)
            {
                Db.RankReward rankReward = new Db.RankReward
                {
                    GuildId = Context.Guild.Id,
                    ReqScore = reqScore,
                    RoleId = role.Id,
                    AdddedBy = Context.User.Id
                };
                _db.Repository.Insert(rankReward);
                await ReplyImage($"Do listy nagórd dodano rangę {role} za {reqScore} punktów");
            }
            else
            {
                if (reqScore == 0)
                {
                    _db.Repository.Delete<Db.RankReward>(x => x.Id == rewardRole.Id);
                    await ReplyImage($"Usunięto z listy nagród rangę {role.Name}.");
                }
                else
                {
                    rewardRole.ReqScore = reqScore;
                    _db.Repository.Update(rewardRole);
                    await ReplyImage($"Rola {role} kosztuje teraz {reqScore} punktów");
                }
            }
        }

        [Command("GetRewardRoles"), Summary("Wyświetla listę nagród za punkty.")]
        public async Task GetRewardRoles()
        {
            var rewards = _db.Repository.Query<Db.RankReward>().Where(x => x.GuildId == Context.Guild.Id).ToEnumerable().OrderBy(x => x.ReqScore);

            StringBuilder sb = new StringBuilder();

            foreach (var reward in rewards)
            {
                var role = Context.Guild.Roles.Where(x => x.Id == reward.RoleId).FirstOrDefault();
                sb.Append($"{role.Name} - {reward.ReqScore}\n");
            }

            await ReplyImage(sb.ToString());
        }

        [Command("AddAdvanceImage"), Summary("Dodaje do wyświetlenia po awansowaniu na kolejny poziom.")]
        public async Task AddAdvanceImage()
        {
            var url = Context.Message.Attachments.FirstOrDefault()?.Url;
            if(url is null)
            {
                await ReplyImage("Brak załączonego obrazka");
                return;
            }

            _db.Repository.Insert(new AdvanceImage
            {
                GuildId = Context.Guild.Id,
                Url = url,
            });

            await ReplyImage($"Dodano {url}");
        }

        [Command("GetAdvanceImages")]
        public async Task GetAdvanceImages()
        {
            var images =_db.Repository.Query<AdvanceImage>().Where(x => x.GuildId == Context.Guild.Id).ToEnumerable();

            foreach(var i in images)
            {
                await ReplyImage($"{i.Id}", url: i.Url);
            }
        }

        [Command("RemoveAdvanceImage")]
        public async Task RemoveAdvanceImage(int id)
        {
            var img = _db.Repository.Query<AdvanceImage>().Where(x => x.GuildId == Context.Guild.Id && x.Id == id).FirstOrDefault();
            
            if(img is null)
            {
                await ReplyImage("Na obecnej gildii brak obrazka o takim id.");
            }
            else
            {
                _db.Repository.Delete<AdvanceImage>(img.Id);
                await ReplyImage("Pomyślnie usunięto obrazek.");
            }            
        }
    }
}

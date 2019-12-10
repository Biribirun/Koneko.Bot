using Discord;
using Discord.Commands;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Koneko.Bot.ModuleBaseExtension;
using Koneko.Bot.DataAccessLayer.Repositories;
using Koneko.Bot.Domain.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;

namespace Koneko.Bot.Administration
{
    [RequireUserPermission(GuildPermission.Administrator, Group = "Permission")]
    [RequireOwner(Group = "Permission")]
    public class AdministrationCommands : ModuleBaseEx
    {
        private readonly StatisticsRepository _statisticsRepository;
        private readonly AdvanceImagesRepository _advanceImagesRepository;
        private readonly MemoryCache _memoryCache;
        public AdministrationCommands(MessageRemoverService responseRemover,
            StatisticsRepository statisticsRepository, AdvanceImagesRepository  advanceImagesRepository, MemoryCache memoryCache) : base(responseRemover)
        {
            _statisticsRepository = statisticsRepository;
            _advanceImagesRepository = advanceImagesRepository;
            _memoryCache = memoryCache;
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

            var rankReward = await _statisticsRepository.GetRankReward(Context.Guild.Id, Context.User.Id);

            if (rankReward is null)
            {
                rankReward = new RankReward
                {
                    GuildId = Context.Guild.Id,
                    ReqScore = reqScore,
                    RoleId = role.Id,
                    AddedBy = Context.User.Id
                };
                await _statisticsRepository.AddRankReward(rankReward);
                await ReplyImage($"Do listy nagórd dodano rangę {role} za {reqScore} punktów");
            }
            else
            {
                if (reqScore == 0)
                {
                    await _statisticsRepository.DeleteRankReward(rankReward);
                    await ReplyImage($"Usunięto z listy nagród rangę {role.Name}.");
                }
                else
                {
                    rankReward.ReqScore = reqScore;
                    await _statisticsRepository.UpdateRankReward(rankReward);
                    await ReplyImage($"Rola {role} kosztuje teraz {reqScore} punktów");
                }
            }
        }

        [Command("GetRewardRoles"), Summary("Wyświetla listę nagród za punkty.")]
        public async Task GetRewardRoles()
        {
            var rewards = await _statisticsRepository.GetRankRewards(Context.Guild.Id);

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

            await _advanceImagesRepository.AddAdvanceImage(new AdvanceImage
            {
                GuildId = Context.Guild.Id,
                Url = url,
            });

            await ReplyImage($"Dodano {url}");
        }

        [Command("GetAdvanceImages")]
        public async Task GetAdvanceImages()
        {
            var images = await _advanceImagesRepository.GetAdvancesImage(Context.Guild.Id);

            foreach (var i in images)
            {
                await ReplyImage($"{i.Id}", url: i.Url);
            }
        }

        [Command("RemoveAdvanceImage")]
        public async Task RemoveAdvanceImage(int id)
        {
            var img = (await _advanceImagesRepository.GetAdvancesImage(Context.Guild.Id)).Where(i => i.Id == id).FirstOrDefault();

            if (img is null)
            {
                await ReplyImage("Na obecnej gildii brak obrazka o takim id.");
            }
            else
            {
                await _advanceImagesRepository.RemoveAdvanceImage(img);
                await ReplyImage("Pomyślnie usunięto obrazek.");
            }            
        }
    }
}

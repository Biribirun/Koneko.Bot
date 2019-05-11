using Discord;
using Discord.Commands;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Discord.WebSocket;

namespace Koneko.Bot
{
    public class Commands : ModuleBase
    {
        DataSource ds = new DataSource();

        [Command("pyrkonowiec"), Summary("Daje rangę pyrkonowiec")]
        public async Task Pyrkonowiec()
        {
            var user = Context.User;
            var role = Context.Guild.Roles.Where(i => i.Name.ToLower().Equals("pyrkonowiec")).FirstOrDefault();

            await (user as SocketGuildUser).AddRoleAsync(role);
            await ReplyImage(description: $"{user.Username} JEDZIE NA PYRKON");
        }

        private async Task ReplyImage(string url = null, string description = "")
        {
            var embed = new EmbedBuilder
            {
                ImageUrl = url,
                Description = description
            };
            await ReplyAsync(embed: embed.Build());
        }

        [Command("baka"), Summary("Nazywa kogoś głupcem.")]
        public async Task Baka([Remainder, Summary("")] IUser user) =>
            await ReplyImage(await ds.GetBaka(), $"{Context.User.Mention} twierdzi że {user.Mention} jest gupi.");

        [Command("poke"), Summary("Zaczepia użytkownika.")]
        public async Task Poke([Remainder, Summary("")] IUser user) =>
            await ReplyImage(await ds.GetPoke(), $"{Context.User.Mention} zaczepia {user.Mention}.");

        [Command("tickle"), Summary("Łaskocze użytkownika.")]
        public async Task Tickle([Remainder, Summary("")] IUser user) =>
            await ReplyImage(await ds.GetTickle(), $"{Context.User.Mention} łaskocze {user.Mention}.");

        [Command("kiss"), Summary("Całuje użytkownika.")]
        public async Task Kiss([Remainder, Summary("")] IUser user) =>
            await ReplyImage(await ds.GetKiss(), $"{Context.User.Mention} całuje {user.Mention}. Miłość rośnie wokół nas.");

        [Command("slap"), Summary("Uderza użytkownika.")]
        public async Task Slap([Remainder, Summary("")] IUser user) =>
            await ReplyImage(await ds.GetSlap(), $"{Context.User.Mention} ma dzisiaj zły dzień i uderza {user.Mention}. To było super efektywne.");

        [Command("cuddle"), Summary("Mizia użytkownika.")]
        public async Task Cuddle([Remainder, Summary("")] IUser user) =>
            await ReplyImage(await ds.GetCuddle(), $"{Context.User.Mention} mizia {user.Mention}.");

        [Command("hug"), Summary("Przytula użytkownika.")]
        public async Task Hug([Remainder, Summary("")] IUser user) =>
            await ReplyImage(await ds.GetHug(), $"{Context.User.Mention} przytula {user.Mention}.");

        [Command("pat"), Summary("Głaska użytkownika.")]
        public async Task Pat([Remainder, Summary("")] IUser user) =>
            await ReplyImage(await ds.GetPat(), $"{Context.User.Mention} głaska po główce {user.Mention}. uwu");

        [Command("feed"), Summary("Karmi użytkownika.")]
        public async Task Feed([Remainder, Summary("")] IUser user) =>
            await ReplyImage(await ds.GetFeed(), $"{Context.User.Mention} karmi {user.Mention}.");

        [Command("avatar"), Summary("Pokazuje awatar użytkoiwnika.")]
        public async Task Avatar([Remainder, Summary("")] IUser user) =>
            await ReplyImage(user.GetAvatarUrl(), $"{user.Mention}");


        [Command("emote"), Summary("Powiększa emotikonę.")]
        public async Task Emote([Remainder, Summary("")] string emote)
        {
            System.Console.WriteLine(emote);
            var e = GuildEmote.TryParse(emote, out Emote emo);
            if (!e)
            {
                await ReplyAsync("Nie udało się :( komenda nie działa na domyślne emotikony.");
                return;
            }
            var embed = new EmbedBuilder
            {
                ImageUrl = emo.Url,
                Description = emote.Replace(":", "")
            };
            await ReplyAsync(embed: embed.Build());
        }

        [Command("help"), Summary("Wyświetla pomoc.")]
        public async Task Help()
        {
            var sb = new StringBuilder();

            foreach (var command in Program.Commands)
            {
                sb.Append($"!{command.Name} - {command.Summary}");

                IEnumerable<string> argumentsinfo = command.Parameters.Select(
                    x => "[" +
                    (x.IsOptional ? "[" : "") +
                    $"[{x.Name}] <{x.Type.Name}>]" +
                    (x.IsOptional ? "]" : ""));

                sb.Append($" {string.Join("\t", argumentsinfo)}\n");
            }

            var embed = new EmbedBuilder
            {
                Description = sb.ToString()
            };
            await ReplyAsync(embed: embed.Build());
        }

    }
}

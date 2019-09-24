using Discord;
using Discord.Commands;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Discord.WebSocket;
using System;
using Koneko.Bot.ModuleBaseExtension;
using Koneko.Bot.Db;

namespace Koneko.Bot.Misc
{
    public class Commands : ModuleBaseEx
    {
        private NekosDs _nekosds = new NekosDs();
        private readonly CommandService _commands;
        
        public Commands(DbConnection dbConnection, CommandService commands) : base(dbConnection)
        {
            _commands = commands;
        }

        [Command("baka"), Summary("Nazywa kogoś głupcem.")]
        public async Task Baka([Remainder, Summary("")] IUser user) =>
            await ReplyImage($"{Context.User.Mention} twierdzi że {user.Mention} jest gupi.", await _nekosds.GetBaka());

        [Command("poke"), Summary("Zaczepia użytkownika.")]
        public async Task Poke([Remainder, Summary("")] IUser user) =>
            await ReplyImage($"{Context.User.Mention} zaczepia {user.Mention}.", await _nekosds.GetPoke());

        [Command("tickle"), Summary("Łaskocze użytkownika.")]
        public async Task Tickle([Remainder, Summary("")] IUser user) =>
            await ReplyImage($"{Context.User.Mention} łaskocze {user.Mention}.", await _nekosds.GetTickle());

        [Command("kiss"), Summary("Całuje użytkownika.")]
        public async Task Kiss([Remainder, Summary("")] IUser user) =>
            await ReplyImage($"{Context.User.Mention} całuje {user.Mention}. Miłość rośnie wokół nas.", await _nekosds.GetKiss());

        [Command("slap"), Summary("Uderza użytkownika.")]
        public async Task Slap([Remainder, Summary("")] IUser user) =>
            await ReplyImage($"{Context.User.Mention} ma dzisiaj zły dzień i uderza {user.Mention}. To było super efektywne.", await _nekosds.GetSlap());

        [Command("cuddle"), Summary("Mizia użytkownika.")]
        public async Task Cuddle([Remainder, Summary("")] IUser user) =>
            await ReplyImage($"{Context.User.Mention} mizia {user.Mention}.", await _nekosds.GetCuddle());

        [Command("hug"), Summary("Przytula użytkownika.")]
        public async Task Hug([Remainder, Summary("")] IUser user) =>
            await ReplyImage($"{Context.User.Mention} przytula {user.Mention}.", await _nekosds.GetHug());

        [Command("pat"), Summary("Głaska użytkownika.")]
        public async Task Pat([Remainder, Summary("")] IUser user) =>
            await ReplyImage($"{Context.User.Mention} głaska po główce {user.Mention}. uwu", await _nekosds.GetPat());

        [Command("feed"), Summary("Karmi użytkownika.")]
        public async Task Feed([Remainder, Summary("")] IUser user) =>
            await ReplyImage($"{Context.User.Mention} karmi {user.Mention}.", await _nekosds.GetFeed());

        [Command("avatar"), Summary("Pokazuje awatar użytkoiwnika.")]
        public async Task Avatar([Remainder, Summary("")] IUser user) =>
            await ReplyImage($"{user.Mention}", user.GetAvatarUrl());


        [Command("emote"), Summary("Powiększa emotikonę.")]
        public async Task Emote([Remainder, Summary("")] string emote)
        {
            Console.WriteLine(emote);
            var e = GuildEmote.TryParse(emote, out Emote emo);
            if (!e)
            {
                return;
            }

            await ReplyImage(emote.Replace(":", ""), emo.Url);
        }

        [Command("help"), Summary("Wyświetla pomoc.")]
        public async Task Help()
        {
            var sb = new StringBuilder();

            foreach (var command in _commands.Commands)
            {
                var preconditionResult = await command.CheckPreconditionsAsync(Context);

                if (!preconditionResult.IsSuccess)
                    continue;                

                sb.Append($"!{command.Name} - {command.Summary}");

                IEnumerable<string> argumentsinfo = command.Parameters.Select(
                    x => "[" +
                    (x.IsOptional ? "[" : "") +
                    $"[{x.Name}] <{x.Type.Name}>]" +
                    (x.IsOptional ? "]" : ""));

                sb.Append($" {string.Join("\t", argumentsinfo)}\n");
            }

            await ReplyImage(description: sb.ToString());
        }

    }
}

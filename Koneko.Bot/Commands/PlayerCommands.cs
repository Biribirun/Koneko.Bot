using Discord;
using Discord.Commands;
using Koneko.Bot.Domain.Models;
using Koneko.Bot.ModuleBaseExtension;
using Koneko.Bot.Preconditions;
using Koneko.Bot.Services;
using Lavalink4NET;
using Lavalink4NET.Events;
using Lavalink4NET.Player;
using Lavalink4NET.Rest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Koneko.Bot.Commands
{
    public class PlayerCommands : ModuleBaseEx
    {
        private readonly PlayerService _playerService;
        public PlayerCommands(MessageRemoverService responseRemover, PlayerService playerService) : base(responseRemover)
        {
            _playerService = playerService;
        }

        [Command("play", RunMode = RunMode.Async)]
        public async Task Play([Remainder]string url)
        {
            if (!await _playerService.EnsureCorrectTextChat(Context))
                return;

            var channel = (Context.User as IGuildUser)?.VoiceChannel;
            if (channel is null)
            {

                await ReplyImage("Musisz być na kanale głosowym.");
                return;
            }
            var myTrack = await _playerService.SearchTrack($"ytsearch:{url}");

            await _playerService.AddToQueue(myTrack, Context.Guild.Id, channel.Id);
        }

        [Command("soundcloud", RunMode = RunMode.Async)]
        public async Task Soundcloud([Remainder]string url)
        {
            if (!await _playerService.EnsureCorrectTextChat(Context))
                return;

            var channel = (Context.User as IGuildUser)?.VoiceChannel;
            if (channel is null)
            {

                await ReplyImage("Musisz być na kanale głosowym.");
                return;
            }
            var myTrack = await _playerService.SearchTrack($"scsearch:{url}");

            await _playerService.AddToQueue(myTrack, Context.Guild.Id, channel.Id);
        }

        [Command("skip", RunMode = RunMode.Async)]
        public async Task Skip()
        {
            if (!await _playerService.EnsureCorrectTextChat(Context))
                return;

            await _playerService.Skip(Context.Guild.Id);
        }

//        [Command("AddFile")]
//        public async Task AddFile(string file)
//        {
//            if (!await _playerService.EnsureCorrectTextChat(Context))
//                return;

//            var channel = (Context.User as IGuildUser)?.VoiceChannel;
//            if (channel is null)
//            {
//                await ReplyImage("Musisz być na kanale głosowym.");
//                return;
//            }

//            var trackInfoJson = @$"{{
//    ""author"" : ""dupa"",
//    ""isStream"" : false,
//    ""isSeekable"" : true,
//    ""uri"" : ""{HttpUtility.JavaScriptStringEncode(file)}"",
//    ""title"": ""chuj"",
//    ""identifier"" : ""dupa""
//}}";

//            LavalinkTrackInfo trackInfo = JsonConvert.DeserializeObject<LavalinkTrackInfo>(trackInfoJson);

//            LavalinkTrack a = new LavalinkTrack("dupa", trackInfo);

//            _ = _playerService.AddToQueue(a, Context.Guild.Id, channel.Id);

//        }

        [Command("Setup")]
        [RequireUserPermission(GuildPermission.Administrator, Group = "Permission")]
        [RequireOwner(Group = "Permission")]
        public async Task Setup(IChannel channel)
        {
            await _playerService.Setup(Context.Guild.Id, channel.Id);
        }

        [Command("like")]
        public async Task Like()
        {

            if (!await _playerService.EnsureCorrectTextChat(Context))
                return;

            await _playerService.LikeCurrentAudio(Context.Guild.Id, Context.User.Id, LikeType.Like);
        }

        [Command("dislike")]
        public async Task Disike()
        {
            if (!await _playerService.EnsureCorrectTextChat(Context))
                return;

            await _playerService.LikeCurrentAudio(Context.Guild.Id, Context.User.Id, LikeType.Dislike);
        }
    }

    
}
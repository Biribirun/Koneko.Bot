using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Koneko.Bot.Context;
using Koneko.Bot.DataAccessLayer.Repositories;
using Koneko.Bot.Domain.Models;
using Koneko.Bot.Helpers;
using Lavalink4NET;
using Lavalink4NET.DiscordNet;
using Lavalink4NET.Player;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koneko.Bot.Services
{
    public class PlayerService
    {
        public class PlayerConfiguration
        {
            public ulong GuildId { get; set; }
            public ulong TextChatId { get; set; }
            public ulong MessageView { get; set; }
            public ulong QueueView { get; set; }

        }

        private readonly IAudioService _audioService;
        private readonly DiscordSocketClient _discordSocketClient;
        private readonly ConcurrentDictionary<ulong, GuildContext> _queues;
        private readonly ConfigurationRepository _configurationRepository;
        private readonly AudioRatingRepository _audioRatingRepository;
        private readonly ContextCache _contextCache;
        private readonly MessageRemoverService _messageRemoverService;

        public PlayerService(IAudioService audioService, ConfigurationRepository configurationRepository, DiscordSocketClient discordSocketClient, 
            AudioRatingRepository audioRatingRepository, ContextCache contextCache, MessageRemoverService messageRemoverService)
        {
            _audioService = audioService;
            _queues = new ConcurrentDictionary<ulong, GuildContext>();
            _configurationRepository = configurationRepository;
            _discordSocketClient = discordSocketClient;
            _audioRatingRepository = audioRatingRepository;
            _contextCache = contextCache;
            _messageRemoverService = messageRemoverService;
        }
        
        private async void UpdatePlayerView(ulong guildId)
        {
            try
            {

                var playerBag = GetPlayerBag(guildId);
                var player = playerBag.Player;

                var configuration = await _configurationRepository.GetConfiguration<PlayerConfiguration>(guildId, "PlayerViewConfiguration");

                if (configuration is null)
                    return;

                var channel = (IMessageChannel)_discordSocketClient.GetChannel(configuration.TextChatId);

                var viewMsg = (IUserMessage)await channel.GetMessageAsync(configuration.MessageView);
                var queueMsg = (IUserMessage)await channel.GetMessageAsync(configuration.QueueView);

                var title = player?.CurrentTrack?.Title ?? "Pusta kolejka oddtwarzania";
                var icon = player?.State == PlayerState.Playing ? "https://cdn.discordapp.com/attachments/536616972159549458/650767033339871255/playpause.png" : "https://cdn.discordapp.com/attachments/650687674356727838/650743303549091851/stop.png";

                var ratings = await _audioRatingRepository.GetAudioRatingForSong(guildId, player.CurrentTrack.Identifier);

                var likes = ratings.Where(x => x.LikeType == LikeType.Like);
                var dislikes = ratings.Where(x => x.LikeType == LikeType.Dislike);

                var embed = new EmbedBuilder
                {
                    ImageUrl = "https://cdn.discordapp.com/attachments/650308145972903940/653310455653072914/pobrany_plik_1.gif",
                    Author = new EmbedAuthorBuilder
                    {
                        Name = $"{title} [⭐ {likes.Count()} ❌ {dislikes.Count()} ]",
                        IconUrl = icon
                    },
                    Color = Color.Teal
                };

                await viewMsg.ModifyAsync(x => x.Embed = embed.Build());

                string queuetext = string.Empty;

                var repeatEmoji = playerBag.Repeat switch
                {
                    RepeatType.NormalQueue => "🚫",
                    RepeatType.Repeat => "🔁",
                    RepeatType.RepeatOne => "🔂",
                    _ => ""
                };

                queuetext += $"**Powtarzanie:** {repeatEmoji}\n";

                queuetext += playerBag.Playlist.Count > 0 ?
                    "**Lista odtwarzania**\n" :
                    "**Lista odtwarzania jest pusta**";

                var next = playerBag.NextSongPointer;
                var plist = playerBag.Playlist;

                queuetext += @$"{string.Join("\n",
                    playerBag.Playlist
                    .Where(x => plist.IndexOf(x) + 1 >= next - 5)
                    .Select(
                        x => x.Format4Player(plist.IndexOf(x) + 1 >= next, (plist.IndexOf(x) + 1 == next))
                        ))}";

                await queueMsg.ModifyAsync(x => x.Content = queuetext);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public async Task<bool> EnsureCorrectTextChat(ICommandContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var result = false;
            var configuration = await _configurationRepository.GetConfiguration<PlayerConfiguration>(context.Guild.Id, "PlayerViewConfiguration");


            if (configuration?.TextChatId == context.Channel.Id)
            {
                result = true;
            }
            else
            {
                if(configuration is null)
                {
                    var embed = new EmbedBuilder
                    {
                        Description = "Player musi być najpierw zainstalowany przez administracje `!setup <nazwa chatu tekstowego do obsługi bota>`, musi też posiadać uprawnienie do usuwania wiadomości"
                    }.Build();
                    var resp = await context.Channel.SendMessageAsync(embed: embed);
                    await _messageRemoverService.SaveBotResponse(context, resp);
                }
                else
                {
                    var channel = (IMessageChannel)_discordSocketClient.GetChannel(configuration.TextChatId);

                    var embed = new EmbedBuilder
                    {
                        Description = $"Usługa odtwarzania muzyki jest obsługiwana tylko na kanale #{channel.Name}"
                    }.Build();
                    var resp = await context.Channel.SendMessageAsync(embed: embed);
                    await _messageRemoverService.SaveBotResponse(context, resp);
                }
            }

            return result;
        }

        public async Task<bool> Setup(ulong guildId, ulong channelId)
        {
            var a = await _configurationRepository.GetConfiguration(guildId, "PlayerViewConfiguration");
            if (true)//(a is null)
            {
                var embed = new EmbedBuilder
                {
                    ImageUrl = "https://cdn.discordapp.com/attachments/650308145972903940/653310455653072914/pobrany_plik_1.gif",
                    Author = new EmbedAuthorBuilder
                    {
                        Name = "Pusta kolejka oddtwarzania",
                        IconUrl = "https://cdn.discordapp.com/attachments/650687674356727838/650743303549091851/stop.png"
                    },
                    Color = Color.Teal
                };

                var channel = (IMessageChannel)_discordSocketClient.GetChannel(channelId);
                var playerMsg = await channel.SendMessageAsync(embed: embed.Build());

                var queueMsg = await channel.SendMessageAsync("**Lista odtwarzania jest pusta**");


                var buttons = new []
                {
                    new Emoji("⏮️"),
                    new Emoji("⏯️"),
                    new Emoji("⏭️"),
                    new Emoji("⏹️"),
                    new Emoji("🔄"),
                    new Emoji("🔀"),
                    new Emoji("⭐"),
                    new Emoji("❌"),
                };
                await playerMsg.AddReactionsAsync(buttons);

                var configuration = new Configuration
                {
                    GuildId = guildId,
                    Key = "PlayerViewConfiguration",
                    Value = JsonConvert.SerializeObject(new PlayerConfiguration
                    {
                        GuildId = guildId,
                        TextChatId = channelId,
                        MessageView = playerMsg.Id,
                        QueueView = queueMsg.Id
                    })
                };

                await _configurationRepository.SetConfiguration(configuration);
            }
            else
            {
                try
                {

                    channelId = Convert.ToUInt64(a);
                }
                finally
                {

                }
            }

            return true;
        }

        public async Task ShufflePlaylist(ulong guildId)
        {
            var context = GetPlayerBag(guildId);


            var prev = context.Playlist.Where(x => context.Playlist.IndexOf(x) + 1 < context.NextSongPointer).OrderBy(x => Guid.NewGuid());

            var next = context.Playlist.Where(x => context.Playlist.IndexOf(x) + 1 > context.NextSongPointer).OrderBy(x => Guid.NewGuid());

            context.Playlist = prev.Union(new[] { context.Player.CurrentTrack }).Union(next).ToList();
            UpdatePlayerView(guildId);
        }

        /// <summary>
        /// true if handled
        /// </summary>
        /// <returns></returns>
        public async Task<bool> HandleReaction(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            var handled = false;

            var userId = arg3.UserId;
            if (_discordSocketClient.CurrentUser.Id == userId)
                return handled;
            var msg = (await arg1.GetOrDownloadAsync());
            if (msg != null)
            {
                SocketGuildChannel channel = msg.Channel as SocketGuildChannel;
                var guildId = channel.Guild.Id;
                var a = (await _configurationRepository.GetConfiguration(guildId, "PlayerViewConfiguration"))?.Value;

                if (!string.IsNullOrEmpty(a))
                {
                    var playerConf = JsonConvert.DeserializeObject<PlayerConfiguration>(a);
                    if (msg.Id == playerConf.MessageView || msg.Id == playerConf.QueueView)
                    {
                        await msg.RemoveReactionAsync(arg3.Emote, arg3.User.Value);
                        if (msg.Id == playerConf.MessageView)
                        {
                            switch(arg3.Emote.Name)
                            {
                                case "⏮️":
                                    _ = GoToPrevious(guildId);
                                    break;

                                case "⏯️":
                                    _ = PlayPause(guildId);
                                    break;

                                case "⏭️":
                                    _ = Skip(guildId);
                                    break;

                                case "⏹️":
                                    _ = Stop(guildId);
                                    break;

                                case "🔄":
                                    _ = SetRepeat(guildId);
                                    break;

                                case "🔀":
                                    _ = ShufflePlaylist(guildId);
                                    break;

                                case "⭐":
                                    _ = LikeCurrentAudio(guildId, userId, LikeType.Like);
                                    break;

                                case "❌":
                                    _ = LikeCurrentAudio(guildId, userId, LikeType.Dislike);
                                    break;

                                default:
                                    break;
                            }
                        }
                    }
                }
            }
            return handled;
        }

        public async Task PlayPause(ulong guildId)
        {

        }
        public async Task Stop(ulong guildId)
        {
            var context = GetPlayerBag(guildId);
            context.Playlist.Clear();
            context.NextSongPointer = 0;
            await context.Player.StopAsync();
            UpdatePlayerView(guildId);
        }

        public async Task SetRepeat(ulong guildId)
        {
            var context = GetPlayerBag(guildId);
            context.Repeat = context.Repeat == RepeatType.NormalQueue ? RepeatType.Repeat : RepeatType.NormalQueue;
            UpdatePlayerView(guildId);
        }

        public async Task GoToPrevious(ulong guildId)
        {
            var context = GetPlayerBag(guildId);
            if (context is null)
                return;

            if(context.NextSongPointer > 1)
            {
                context.NextSongPointer -= 2;
                await ConsumeQueue(guildId);
            }
        }

        public async Task LikeCurrentAudio(ulong guildId, ulong userId, LikeType likeType)
        {
            var playerBag = GetPlayerBag(guildId);
            
                var ar = new AudioRating
                {
                    AudioIdentifier = playerBag.Player.CurrentTrack.Identifier,
                    GuildId = guildId,
                    UserId = userId,
                    LikeType = likeType
                };

            await _audioRatingRepository.SetAudioRating(ar);
            UpdatePlayerView(guildId);
        }

        public async Task<bool> AddToQueue(LavalinkTrack track, ulong guildId, ulong voice)
        {
            if(track is null)
            {

            }
            var queue = GetOrCreatePlayerBag(guildId, voice);
            queue.Playlist.Add(track);

            var player = queue.Player;

            if(player.State == PlayerState.Playing)
            {
                UpdatePlayerView(guildId);
            }
            else
            {
                return await ConsumeQueue(guildId);
            }
                    
            return false;
        }

        private GuildContext GetOrCreatePlayerBag(ulong guildId, ulong voice)
        {
            return _queues.GetOrAdd(guildId, (x) =>
            {
                return Task.Run(async () => await CreatePlayerBag(guildId, voice)).Result;
            });
        }

        private GuildContext GetPlayerBag(ulong guildId)
        {
            return _queues.TryGetValue(guildId, out GuildContext playerBag) ? playerBag : null;
        }

        protected async Task<bool> ConsumeQueue(ulong guildId)
        {
            var result = true;
            var playerBag = GetPlayerBag(guildId);
            var player = playerBag.Player;
            var queue = playerBag.Playlist;

            if (queue is null)
                return false;


            var track = queue.ElementAtOrDefault(playerBag.NextSongPointer);

            if (track != null)
            {
                if (player.State == PlayerState.Destroyed)
                {
                    await player.ConnectAsync(player.VoiceChannelId.Value);
                }
                await player.PlayAsync(track);

                playerBag.NextSongPointer++;
            }
            else
            {
                await player.StopAsync(disconnect: false);
                result = false;
            }

            UpdatePlayerView(guildId);
            return result;
        }

        public async Task<LavalinkTrack> SearchTrack(string query)
        {
            return await _audioService.GetTrackAsync(query);
        }

        internal async Task<bool> Skip(ulong guildId)
        {
            bool consumed = false;
            var playerBag = GetPlayerBag(guildId);
            if (playerBag.NextTrack == null)
            {
                if (playerBag.Repeat == RepeatType.Repeat)
                {
                    playerBag.NextSongPointer = 0;
                }
            }
            if (playerBag.NextTrack != null)
            {
                consumed = await ConsumeQueue(guildId);
            }
            else
            {

            }
            return consumed;
        }

        private async Task<LavalinkPlayerEx> CreatePlayer(ulong guildId, ulong voiceId)
        {
            var player = _audioService.GetPlayer<LavalinkPlayerEx>(guildId);
            if (player is null)
            {
                player = await _audioService.JoinAsync<LavalinkPlayerEx>(guildId, voiceId, selfDeaf: true);
                player.TrackEnded += Player_TrackEnded;
            }
            return player;
        }

        private async Task<GuildContext> CreatePlayerBag(ulong guildId, ulong voiceId)
        {
            var context = _contextCache.GetGuildContext(guildId);

            context.Player = await CreatePlayer(guildId, voiceId);
            return context;
        }

        private async Task<bool> Player_TrackEnded(Lavalink4NET.Events.TrackEndEventArgs eventArgs)
        {
            bool consumed = false;
            ulong guildId = eventArgs.Player.GuildId;
            switch (eventArgs?.Reason)
            {
                case TrackEndReason.Finished:
                    Console.WriteLine($"Track finished {JsonConvert.SerializeObject(eventArgs)}");
                    var playerBag = GetPlayerBag(guildId);
                    consumed = await Skip(guildId);
                    break;

                default:
                    break;
            }
            if(!consumed)
            {
                UpdatePlayerView(guildId);
            }
            return consumed;
        }
    }
}

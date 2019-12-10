using Koneko.Bot.Services;
using Lavalink4NET.Player;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Koneko.Bot.Context
{
    public class GuildContext
    {
        public GuildContext(ulong guildId)
        {
            GuildId = guildId;
            Playlist = new List<LavalinkTrack>();
            NextSongPointer = 0;
        }
        //public ConcurrentQueue<LavalinkTrack> Queue { get; set; }
        //public ConcurrentStack<LavalinkTrack> PrevTracks { get; set; }
        public List<LavalinkTrack> Playlist { get; set; }
        public int NextSongPointer { get; set; }
        public LavalinkTrack NextTrack => Playlist.ElementAtOrDefault(NextSongPointer);
        public LavalinkPlayerEx Player { get; set; }
        public ulong GuildId { get; set; }
        public RepeatType Repeat { get; set; }
    }
    public enum RepeatType
    {
        NormalQueue,
        Repeat,
        RepeatOne
    }
}

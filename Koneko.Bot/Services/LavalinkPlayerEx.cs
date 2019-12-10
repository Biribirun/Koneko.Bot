using Lavalink4NET;
using Lavalink4NET.Events;
using Lavalink4NET.Player;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Koneko.Bot.Services
{
    public delegate Task<bool> TrackEndAsync(TrackEndEventArgs eventArgs);
    public class LavalinkPlayerEx : LavalinkPlayer
    {
        public event TrackEndAsync TrackEnded;
        private bool Finished = true;

        public LavalinkPlayerEx(LavalinkSocket lavalinkSocket, IDiscordClientWrapper client, ulong guildId, bool disconnectOnStop) : base(lavalinkSocket, client, guildId, false)
        {
        }        
        public override async Task OnTrackEndAsync(TrackEndEventArgs eventArgs)
        {
            if (!await TrackEnded(eventArgs))
                Finished = true;
        }

        public override Task PlayAsync(LavalinkTrack track, TimeSpan? startTime = null, TimeSpan? endTime = null, bool noReplace = false)
        {
            Finished = false;
            return base.PlayAsync(track, startTime, endTime, noReplace);
        }

        public new PlayerState State =>
            base.State == PlayerState.Destroyed ? PlayerState.Destroyed :(
            Finished ? PlayerState.NotPlaying : base.State);
    }
}

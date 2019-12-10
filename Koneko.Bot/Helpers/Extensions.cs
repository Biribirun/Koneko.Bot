using Lavalink4NET.Player;
using System;
using System.Collections.Generic;
using System.Text;

namespace Koneko.Bot.Helpers
{
    public static class Extensions
    {
        public static T RandomElement<T>(this IEnumerable<T> source, Random random)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (random is null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            T current = default(T);
            int n = 0;
            foreach (T element in source)
            {
                n++;
                if (random.Next(n) == 0)
                    current = element;
            }
            return current;
        }

        public static T RandomElement<T>(this IList<T> source, Random random)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (random is null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            var current = source[random.Next(source.Count)];

            return current;
        }
        public static string Format4Player(this LavalinkTrack track, bool next, bool current)
        {
            if (track is null)
            {
                throw new ArgumentNullException(nameof(track));
            }

            var d = track.Duration;
            var tHours = (int)d.TotalHours;


            var curr = current ? "\\▶️ " : "";

            var format = curr + (tHours > 0 ? "[{1:00}:" : "[");
            format += "{2:00}:{3:00}] {4}";
            format = next ? $"**{format}**" : $"*{format}*";


            return string.Format(format, curr, tHours, d.Minutes, d.Seconds, track.Title);
        }
    }
}

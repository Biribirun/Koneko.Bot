using System;
using System.Collections.Generic;
using System.Text;

namespace Koneko.Bot.Helpers
{
    public static class Extensions
    {
        public static T RandomElement<T>(this IEnumerable<T> source, Random random)
        {
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

    }
}

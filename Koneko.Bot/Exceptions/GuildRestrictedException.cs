using System;
using System.Collections.Generic;
using System.Text;

namespace Koneko.Bot.Exceptions
{
    public class GuildRestrictedException : Exception
    {
        public GuildRestrictedException() : base()
        {
        }
        public GuildRestrictedException(string message) : base(message)
        {
        }

        public GuildRestrictedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

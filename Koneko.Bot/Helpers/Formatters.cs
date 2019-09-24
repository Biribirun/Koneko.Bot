using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;

namespace Koneko.Bot.Helpers
{
    public class Formatters
    {
        public static string GetUserName(IUser user)
        {
            if(user is null)
            {
                return $"Użytkownik opuścił serwer ID: {user.Id}";
            }
            var u = (user as SocketGuildUser);
            return string.IsNullOrEmpty(u.Nickname) ? u.Username : u.Nickname;
        }
    }
}

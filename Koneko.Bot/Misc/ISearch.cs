using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Koneko.Bot.Misc
{
    public interface ISearch
    {
        Task<IEnumerable<string>> FindImages(string phrase);
    }
}

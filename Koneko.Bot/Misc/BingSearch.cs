using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Search.WebSearch;
using Microsoft.Azure.CognitiveServices.Search.WebSearch.Models;

namespace Koneko.Bot.Misc
{
    class BingSearch : ISearch
    {
        public Task<IEnumerable<string>> FindImages(string phrase)
        {
            var client = new WebSearchClient(new ApiKeyServiceClientCredentials("YOUR_SUBSCRIPTION_KEY"));
            client.Web.SearchAsync(query: phrase, market:"pl-pl", count: 10);

        }
    }
}

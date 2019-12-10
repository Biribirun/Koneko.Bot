using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;

namespace Koneko.Bot
{
    class NekosDs
    {
        private class Rootobject
        {
            public string Url { get; set; }
        }

        private readonly Uri API = new Uri("https://nekos.life/api/v2/img/");

        public async Task<string> GetImage(string type)
        {
            using HttpClient http = new HttpClient();
            var result = await http.GetAsync(new Uri(API, type));
            var json = result.Content.ReadAsStringAsync().Result;
            var img = Newtonsoft.Json.JsonConvert.DeserializeObject<Rootobject>(json);
            return img.Url;
        }

        public async Task<string> GetBaka() => await GetImage("baka");

        public async Task<string> GetPoke() => await GetImage("poke");

        public async Task<string> GetTickle() => await GetImage("tickle");

        public async Task<string> GetKiss() => await GetImage("kiss");

        public async Task<string> GetSlap() => await GetImage("slap");

        public async Task<string> GetCuddle() => await GetImage("cuddle");

        public async Task<string> GetHug() => await GetImage("hug");

        public async Task<string> GetPat() => await GetImage("pat");

        public async Task<string> GetFeed() => await GetImage("feed");
    }
}


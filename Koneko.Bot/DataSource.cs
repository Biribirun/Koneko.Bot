using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;

namespace Koneko.Bot
{
    public class Rootobject
    {
        public string url { get; set; }
    }

    class DataSource
    {
        HttpClient http = new HttpClient();
        private readonly string API = "https://nekos.life/api/v2/img/";


        public DataSource()
        {
        }

        public async Task<string> GetImage(string type)
        {
            var result = http.GetAsync($"{API}{type}").Result;
            var json = result.Content.ReadAsStringAsync().Result;
            var img = Newtonsoft.Json.JsonConvert.DeserializeObject<Rootobject>(json);
            return img.url;
        }

        public async Task<string> GetBaka()
        {
            return await GetImage("baka");
        }

        public async Task<string> GetPoke()
        {
            return await GetImage("poke");
        }

        public async Task<string> GetTickle()
        {
            return await GetImage("tickle");
        }

        public async Task<string> GetKiss()
        {
            return await GetImage("kiss");
        }

        public async Task<string> GetSlap()
        {
            return await GetImage("slap");
        }

        public async Task<string> GetCuddle()
        {
            return await GetImage("cuddle");
        }

        public async Task<string> GetHug()
        {
            return await GetImage("hug");
        }

        public async Task<string> GetPat()
        {
            return await GetImage("pat");
        }

        public async Task<string> GetFeed()
        {
            return await GetImage("feed");
        }
    }
}


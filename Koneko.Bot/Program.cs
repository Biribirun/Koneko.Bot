using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Koneko.Bot.Db;
using Koneko.Bot.Misc;

namespace Koneko.Bot
{
    class Program
    {
        private IServiceProvider services;
        static void Main(string[] args)
            => new Program().MainAsync(args).GetAwaiter().GetResult();

        public async Task MainAsync(string[] args)
        {
            services = new ServiceCollection()
                .AddSingleton<BotMain>()
                .AddSingleton<DbConnection>()
                .AddScoped<ResponseRemover>()
                .AddScoped<ErrorHandler>()
                .AddScoped<Statistics>()
                .AddScoped<ISearch, BingSearch>()
                .BuildServiceProvider();

            await services.GetService<BotMain>().Main(args);
        }
    }
}

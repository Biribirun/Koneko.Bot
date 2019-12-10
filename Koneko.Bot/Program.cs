using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;


namespace Koneko.Bot
{
    public class Program
    {
        static int Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("Config.json")
                .Build();

            var webHost = CreateWebHostBuilder(args, configuration).Build();

            try
            {
                webHost.Run();
                return 0;
            }
            catch (Exception ex)
            {
                return ex.HResult;
            }
            finally
            {

            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args, IConfiguration config) =>
            WebHost.CreateDefaultBuilder(args)
                .UseConfiguration(config)
                .UseStartup<Startup>();
    }

}

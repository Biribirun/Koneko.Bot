using Koneko.Bot.DataAccessLayer.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;

namespace Koneko.Bot
{
    public class Startup
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            _configuration = configuration;
            _hostingEnvironment = hostingEnvironment;
        }

        
        public void ConfigureServices(IServiceCollection services)
        {
            var conStr = _configuration.GetConnectionString("KonekoDB");

            SqlConnectionStringBuilder efBuilder = new SqlConnectionStringBuilder(conStr);
            efBuilder.UserID = _configuration["UserID"];
            efBuilder.Password = _configuration["Password"];

            services.AddDbContext<KonekoContext>(options =>
                options.UseSqlServer(efBuilder.ConnectionString), ServiceLifetime.Transient);

            services.AddScoped(x => _configuration);

            services.AddKoneko(_configuration);
            services.AddSingleton<MemoryCache>();
            services.BuildServiceProvider();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {

        }
    }
}

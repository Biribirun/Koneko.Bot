using Discord.WebSocket;
using Koneko.Bot.Context;
using Koneko.Bot.DataAccessLayer.Data;
using Koneko.Bot.DataAccessLayer.Repositories;
using Koneko.Bot.Services;
using Lavalink4NET;
using Lavalink4NET.DiscordNet;
using Lavalink4NET.Tracking;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Koneko.Bot
{
    internal static class BotServices
    {
        public static IServiceCollection AddKoneko(this IServiceCollection services, IConfiguration configuration)
        {
            //services.AddSingleton(
            //    provider => new DiscordSocketClient(config: new DiscordSocketConfig
            //    {
            //        AlwaysDownloadUsers = true
            //    }));

            var inactiveOptions = new InactivityTrackingOptions
            {
                // Disconnect the player after 10 seconds if it inactive.
                DisconnectDelay = TimeSpan.FromSeconds(60),

                // Check every 4 seconds for an inactive player.
                PollInterval = TimeSpan.FromSeconds(4),

                // Start Tracking after calling the constructor.
                TrackInactivity = true
            };

            services.AddSingleton<BotMain>()
                .AddSingleton<ContextCache>()
                .AddScoped<MessageRemoverService>()
                .AddScoped<ErrorHandlerService>()
                .AddScoped<PlayerService>()
                .AddTransient<AdvanceImagesRepository>()
                .AddTransient<BotResponsesRepository>()
                .AddTransient<AudioRatingRepository>()
                .AddTransient<ConfigurationRepository>()
                .AddTransient<StatisticsRepository>()
                .AddSingleton<DiscordSocketClient>(
                    provider => new DiscordSocketClient(config: new DiscordSocketConfig
                    {
                        AlwaysDownloadUsers = true
                    }))
                .AddSingleton<IAudioService, LavalinkNode>()
                .AddSingleton<IDiscordClientWrapper, DiscordClientWrapper>()
	            .AddSingleton(new LavalinkNodeOptions {
                    RestUri = configuration["playerConfiguration:RestUri"],
                    WebSocketUri = configuration["playerConfiguration:WebSocketUri"],
                    Password = configuration["playerConfiguration:Password"],
                    
                })
                .AddSingleton(inactiveOptions)
                .AddSingleton<InactivityTrackingService>()        
                .AddScoped<Statistics>();

            services.AddHostedService<BotMain>();

            return services;
        }

    }
}

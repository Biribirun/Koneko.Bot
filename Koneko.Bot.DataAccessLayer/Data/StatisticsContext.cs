using Koneko.Bot.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Text;

namespace Koneko.Bot.DataAccessLayer.Data
{
    public class KonekoContext : DbContext
    {
        public KonekoContext(DbContextOptions<KonekoContext> options)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserStatistic>()
                .HasIndex(b => new { b.GuildId, b.UserId });

            modelBuilder.Entity<RankReward>()
                .HasIndex(b => new { b.GuildId });

            modelBuilder.Entity<AdvanceImage>()
                .HasIndex(b => new { b.GuildId });

            modelBuilder.Entity<BotResponse>()
                .HasIndex(b => new { b.GuildId });

            modelBuilder.Entity<Configuration>()
                .HasIndex(b => new { b.GuildId, b.Key });

            modelBuilder.Entity<Configuration>()
                .HasKey(b => new { b.GuildId, b.Key });

            modelBuilder.Entity<AudioRating>()
                .HasKey(b => new { b.GuildId, b.UserId, b.AudioIdentifier });

        }

        public DbSet<UserStatistic> UserStatistics { get; set; }
        public DbSet<RankReward> RankRewards { get; set; }
        public DbSet<BotResponse> BotResponses { get; set; }
        public DbSet<AdvanceImage> AdvanceImages { get; set; }
        public DbSet<Configuration> Configurations { get; set; }
        public DbSet<AudioRating> AudioRatings { get; set; }
    }

    public class KonekoContextFactory : IDesignTimeDbContextFactory<KonekoContext>
    {
        KonekoContext IDesignTimeDbContextFactory<KonekoContext>.CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<KonekoContext>();
            optionsBuilder.UseSqlServer("<connection string>");

            return new KonekoContext(optionsBuilder.Options);
        }
    }
}
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Runtime;
using System.Threading.Tasks;
using DiscordBot.Extensions;
using DiscordBot.Modules;
using DiscordBot.MusicPlayer.Config;
using DiscordBot.MusicPlayer.Factories;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.VoiceNext;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DiscordBot;

using static Environment;
using static ServiceLifetime;

[ExcludeFromCodeCoverage]
internal static class Program
{
    public static async Task Main()
    {
        GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

        //gets the enviroment to be used when getting the appsettings
        var enviroment = GetEnvironmentVariable("Environment") ??
                         "No environment found, using default appsettings";

        Console.WriteLine(enviroment);

        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", true, true)
            .AddJsonFile($"appsettings.{enviroment}.json", true)
            .Build();

        var discord = new DiscordShardedClient(new DiscordConfiguration
        {
            Token = GetEnvironmentVariable("Token") ?? config["Token"],
            TokenType = TokenType.Bot,
            //logger factory to log to console
            LoggerFactory = LoggerFactory.Create(i => i.AddConsole().SetMinimumLevel(LogLevel.Debug))
        });

        var commands = await discord.UseCommandsNextAsync(new CommandsNextConfiguration
        {
            StringPrefixes = new[] {GetEnvironmentVariable("Prefix") ?? config["Prefix"]},

            Services = new ServiceCollection()
                .AddPlayer(i => i
                    .WithLifeTime(Scoped)
                    .WithPlayerBufferFactory(PlayerBufferFactories.CreateMatroska)
                    .WithSapisid(GetEnvironmentVariable("Sapisid") ?? config["Sapisid"])
                    .WithPsid(GetEnvironmentVariable("Psid") ?? config["Psid"])
                )
                .AddControllers()
                .AddMediatR(i => i.AsScoped(), Assembly.GetExecutingAssembly())
                .BuildServiceProvider()
        });

        commands.RegisterCommands<MusicModule>();


        var voiceNextConfig = new VoiceNextConfiguration
        {
            EnableIncoming = false,
            PacketQueueSize = GetEnvironmentVariable("PacketQueueSize").ToIntOrNull() ?? config["PacketQueueSize"].ToIntOrNull() ?? 25
        };

        await discord.UseVoiceNextAsync(voiceNextConfig);

        await discord.StartAsync();

        await Task.Delay(-1);
    }
}
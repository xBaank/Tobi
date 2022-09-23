using System.Runtime;
using DiscordBot.MusicPlayer.Buffers;

namespace DiscordBot;

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.VoiceNext;
using Extensions;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Modules;
using MusicPlayer.Config;
using MusicPlayer.Factories;
using static System.Environment;
using static Microsoft.Extensions.DependencyInjection.ServiceLifetime;

[ExcludeFromCodeCoverage]
internal static class Program
{
    public static async Task Main()
    {
        //gets the enviroment to be used when getting the appsettings
        var enviroment = GetEnvironmentVariable("Environment") ??
                         "No environment found, using default appsettings";
        Console.WriteLine(enviroment);

        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", true, true)
            .AddJsonFile($"appsettings.{enviroment}.json", true)
            .Build();
        
        var lowLactency = (GetEnvironmentVariable("LowLatency") ?? config["LowLatency"] ?? string.Empty) == "true";
        if(lowLactency)
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
        
        var buffer = GetEnvironmentVariable("Buffer") ?? config["Buffer"] ?? string.Empty;
        
        
        PlayerBufferFactory bufferFactory = PlayerBufferFactories.CreateMatroska;
        
        if(buffer == "Ffmpeg")
            bufferFactory = PlayerBufferFactories.CreateFFmpeg;


        var discord = new DiscordShardedClient(new DiscordConfiguration
        {
            Token = GetEnvironmentVariable("Token") ?? config["Token"],
            TokenType = TokenType.Bot,
            //logger factory to log to console
            LoggerFactory = LoggerFactory.Create(i => i.AddConsole().SetMinimumLevel(LogLevel.Debug)),
        });

        var commands = await discord.UseCommandsNextAsync(new CommandsNextConfiguration
        {
            StringPrefixes = new[] { GetEnvironmentVariable("Prefix") ?? config["Prefix"] },

            Services = new ServiceCollection()
                .AddPlayer(i => i
                    .WithLifeTime(Scoped)
                    .WithPlayerBufferFactory(bufferFactory)
                    .WithSapisid(GetEnvironmentVariable("Sapisid") ?? config["Sapisid"])
                    .WithPsid(GetEnvironmentVariable("Psid") ?? config["Psid"])
                )
                .AddControllers()
                .AddMediatR(i => i.AsScoped(), Assembly.GetExecutingAssembly())
                .BuildServiceProvider(),
        });

        commands.RegisterCommands<MusicModule>();

        var voiceNextConfig = new VoiceNextConfiguration
        {
            EnableIncoming = false,
            PacketQueueSize = GetEnvironmentVariable("PacketQueueSize").ToIntOrNull() ?? config["PacketQueueSize"].ToIntOrNull() ?? 25,
        };

        await discord.UseVoiceNextAsync(voiceNextConfig);

        await discord.StartAsync();

        await Task.Delay(-1);
    }
}

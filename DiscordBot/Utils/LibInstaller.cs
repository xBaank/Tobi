using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CliWrap;

namespace DiscordBot.Utils;

public static class LibInstaller
{
    public static async Task InstallLibsAsync()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return;
        await AptUpdate();
        await Task.WhenAll(InstallSodiumAsync(), InstallOpusAsync());
    }

    private static async Task AptUpdate() => await Cli.Wrap("apt")
        .WithArguments(["update"])
        .WithStandardOutputPipe(PipeTarget.ToDelegate(Console.WriteLine))
        .ExecuteAsync();

    private static async Task InstallSodiumAsync() => await Cli.Wrap("apt")
        .WithArguments(["install", "libsodium-dev", "-y"])
        .WithStandardOutputPipe(PipeTarget.ToDelegate(Console.WriteLine))
        .ExecuteAsync();

    private static async Task InstallOpusAsync() => await Cli.Wrap("apt")
        .WithArguments(["install", "libopus-dev", "-y"])
        .WithStandardOutputPipe(PipeTarget.ToDelegate(Console.WriteLine))
        .ExecuteAsync();
}
namespace DiscordBot.Utils;

using System.Diagnostics.CodeAnalysis;
using System.IO;
using FFmpeg.AutoGen;
using static System.OperatingSystem;

[ExcludeFromCodeCoverage]
public static class FFmpegUtils
{
    public static void SetUpFFmpeg(string? path = null)
    {
        if (path is not null)
        {
            ffmpeg.RootPath = Directory.GetCurrentDirectory() + path;
            return;
        }


        if (IsWindows())
            ffmpeg.RootPath = Directory.GetCurrentDirectory() + "/FFmpegBinaries";

        if (IsLinux())
        {
            ffmpeg.RootPath = @"/usr/lib/x86_64-linux-gnu/";
        }
    }
}

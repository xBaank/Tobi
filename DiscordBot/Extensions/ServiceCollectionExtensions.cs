namespace DiscordBot.Extensions;

using Controllers;
using Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddControllers(this IServiceCollection serviceCollection) => serviceCollection
        .AddScoped<IMusicController, MusicController>();
}

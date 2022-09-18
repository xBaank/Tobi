namespace DiscordBot.Extensions;

public static class StringExtensions
{
    public static int? ToIntOrNull(this string? value)
    {
        if(value is null)
            return null;
        
        var result = int.TryParse(value, out var intValue);
        return result ? intValue : null;
    }
}
using System.Security.Cryptography;
using System.Text;

namespace DiscordBot.MusicPlayer.Config;

using Factories;
using Microsoft.Extensions.DependencyInjection;

public class PlayerConfig
{
    public ServiceLifetime Lifetime = ServiceLifetime.Transient;

    public PlayerBufferFactory PlayerBufferFactory = PlayerBufferFactories.CreateMatroska;

    //Cookie used for youtube age restriction
    public string? Psid;

    //Cookie used for youtube age restriction
    public string? Sapisid;

    public PlayerConfig WithLifeTime(ServiceLifetime lifetime)
    {
        Lifetime = lifetime;
        return this;
    }

    public PlayerConfig WithPlayerBufferFactory(PlayerBufferFactory factory)
    {
        PlayerBufferFactory = factory;
        return this;
    }

    public PlayerConfig WithSapisid(string? sapisid)
    {
        Sapisid = sapisid;
        return this;
    }

    public PlayerConfig WithPsid(string? psid)
    {
        Psid = psid;
        return this;
    }
}


public class AuthHandler : DelegatingHandler
{
    public AuthHandler(string papisid, string psid)
    {
        Papisid = papisid;
        Psid = psid;
        InnerHandler = new HttpClientHandler();
    }

    private string Papisid { get; }

    private string Psid { get; }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) 
    {
        if(string.IsNullOrWhiteSpace(Papisid) || string.IsNullOrWhiteSpace(Psid))
            return base.SendAsync(request, cancellationToken);
        
        const string origin = "https://www.youtube.com";
        
        request.Headers.Remove("Cookie");
        request.Headers.Remove("Authorization");
        request.Headers.Remove("Origin");
        request.Headers.Remove("X-Origin");
        request.Headers.Remove("Referer");
        
        request.Headers.Add("Cookie", $"CONSENT=YES+cb; YSC=DwKYllHNwuw; __Secure-3PAPISID={Papisid}; __Secure-3PSID={Psid}");
        request.Headers.Add("Authorization", $"SAPISIDHASH {GenerateSidBasedAuth(Papisid, origin)}");
        request.Headers.Add("Origin", origin);
        request.Headers.Add("X-Origin", origin);
        request.Headers.Add("Referer", origin);
        
        return base.SendAsync(request, cancellationToken);
    }
    
    private static string GenerateSidBasedAuth(string sid, string origin)
    {
        var date = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var timestamp = date / 1000;
        var sidHash = Hash($"{timestamp} {sid} {origin}");
        return $"{timestamp}_{sidHash}";
    }

    private static string Hash(string input)
    {
        var hash = SHA1.HashData(Encoding.UTF8.GetBytes(input));
        return string.Concat(hash.Select(b => b.ToString("x2")));
    }
}

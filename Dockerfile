FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0-jammy  AS build
ARG TARGETARCH
WORKDIR /src
COPY ["DiscordBot/DiscordBot.csproj", "DiscordBot/"]
COPY ["DiscordBot.MusicPlayer/DiscordBot.MusicPlayer.csproj", "DiscordBot.MusicPlayer/"]
RUN dotnet restore "DiscordBot/DiscordBot.csproj" -a $TARGETARCH
COPY . .
WORKDIR "/src/DiscordBot"
RUN dotnet publish "DiscordBot.csproj" -a $TARGETARCH --self-contained false -c Release --no-restore -o /app/publish

FROM  mcr.microsoft.com/dotnet/runtime:8.0-jammy
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DiscordBot.dll"]

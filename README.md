
<h1 align="center">
  <br>
  <img src="http://creamostuvideo.com/wp-content/uploads/2021/05/discord-logo.png" alt="Markdownify" width="200">
  <br>
  Tobi
  <br>
</h1>

<p align="center"> 
Tobi is a discord music bot that uses low memory and cpu usage.
</p>

<center>
  <a href="https://hub.docker.com/repository/docker/xbank/tobi-bot"><img src="https://img.shields.io/docker/v/xbank/tobi-bot/latest" alt="Markdownify"></a>
  <a href="https://github.com/xBaank/Tobi/actions"><img alt="GitHub Workflow Status (branch)" src="https://img.shields.io/github/actions/workflow/status/xBaank/Tobi/dotnet.yml?branch=main"></a>
</center>


## Comands
- help
- loop
- pause
- play
- resume
- seek
- skip
- stop
- join

## Screenshots
![image](.assets/help.png)
![image](.assets/play.png)
![image](.assets/seek.png)
![image](.assets/pause.png)
![image](.assets/resume.png)
![image](.assets/loop.png)
![image](.assets/skip.png)

## Key Features
- Low memory usage
- Low cpu usage
- Fully async
- Low loading time

## Sources
| Sources  | State |
| ------------- | --- |
| Youtube  | DONE |
| Spotify  | TODO |
| SoundCloud  | TODO |

## Enviroment variables
| Variables  | Required |
| ------------- | --------- |
| Token  | &#9745; |
| Prefix  | &#9745; |
| Sapisid  | &#9744; |
| Psid  | &#9744; |
| PacketQueueSize  |  &#9744; |

> **Warning**
> `Sapisid` and `Psid` are youtube cookies required to make authenticated requests to `www.youtube.com`. 

## Docker
There is a docker image to self host.

To run it use 
```bash
docker run -d -e Token=PutHereYourToken -e Prefix=! xbank/tobi-bot:latest
```

using Comet.Shared;
using Comet.Web.App.Data;
using Comet.Web.App.Data.Entities;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Comet.Web.App.Services.Discord
{
    public class DiscordService : IHostedService
    {
        private static IServiceProvider _serviceProvider;

        private DiscordSocketClient mClient;
        private CommandService mCommandService;

        private IServiceProvider mInternalServiceProvider;

        public ulong MainChannelId { get; private set; }

        private readonly string mDiscordToken;
        private readonly IServiceScopeFactory mServiceScopeFactory;
        private readonly ILogger<DiscordService> mLogger;

        private ApplicationDbContext Context
        {
            get
            {
                var scope = mServiceScopeFactory.CreateScope();
                var services = scope.ServiceProvider;
                return services.GetRequiredService<ApplicationDbContext>();
            }
        }

        public DiscordService(IConfiguration config, IServiceScopeFactory service, ILogger<DiscordService> logger)
        {
            mDiscordToken = config["Discord:Token"];
            mServiceScopeFactory = service;
            mLogger = logger;
        }

        public bool IsReady => mClient?.ConnectionState == ConnectionState.Connected;

        // If any services require the client, or the CommandService, or something else you keep on hand,
        // pass them as parameters into this method as needed.
        // If this method is getting pretty long, you can seperate it out into another file using partials.
        private static IServiceProvider ConfigureServices()
        {
            var map = new ServiceCollection();

            // When all your required services are in the collection, build the container.
            // Tip: There's an overload taking in a 'validateScopes' bool to make sure
            // you haven't made any mistakes in your dependency graph.
            return map.BuildServiceProvider();
        }

        private async Task ClientOnReadyAsync()
        {
            await mClient.SetGameAsync("World Conquer Online");
            await mClient.SetStatusAsync(UserStatus.DoNotDisturb);

            await using var context = Context;
            MainChannelId = (Context.DiscordChannels.FirstOrDefault(x => x.Default) ?? context.DiscordChannels.FirstOrDefault())?.ChannelId ?? 0;

            if (MainChannelId != 0 && mClient.GetChannel(MainChannelId) is SocketTextChannel channel)
            {
                var embed = new EmbedBuilder
                {
                    Author = new EmbedAuthorBuilder
                    {
                        IconUrl = mClient.CurrentUser.GetAvatarUrl(),
                        Name = mClient.CurrentUser.Username
                    },
                    Timestamp = DateTimeOffset.Now,
                    Color = Color.Default,
                    Description = "",
                    Footer = new EmbedFooterBuilder()
                        .WithText("Comet Discord Bot v1.0.0a"),
                    Title = "Comet[PM] está ONLINE!",
                    Fields = new List<EmbedFieldBuilder>
                    {
                        new EmbedFieldBuilder()
                            .WithName("Apresentação")
                            .WithValue($"Olá Conquistadores!\nMeu nome é Comet[PM] e eu estou aqui para mantê-lo atualizado com as informações dos servidores."),
                        new EmbedFieldBuilder()
                            .WithName("Temporário")
                            .WithValue("Por enquanto, não tenho funções. Apenas registrarei as mensagens enviadas por aqui, mas em breve terei comandos para vocês." +
                                       " Caso tenham sugestões, sintam-se livres para mandar mensagens.")
                        //new EmbedFieldBuilder()
                        //    .WithName("Servidor de Login")
                        //    .WithValue("Global [Online]"),
                        //new EmbedFieldBuilder()
                        //    .WithName("Servidores Conectados")
                        //    .WithValue("Brasil [0/1]")
                    },
                    ThumbnailUrl = "https://worldconquer.online/assets/images/logo.png"
                }.Build();
                await channel.SendMessageAsync("Opa! O bot do servidor está online! Em breve teremos comandos para vocês terem um feedback instantaneo do servidor.",
                    false, embed, RequestOptions.Default, AllowedMentions.None);
            }
        }

        private async Task OnCommandReceiveAsync(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            if (message == null)
                return;

            if (message.Author.Id == mClient.CurrentUser.Id || message.Author.IsBot)
                return;

            int pos = 0;
            if (message.HasCharPrefix('/', ref pos) || message.HasMentionPrefix(mClient.CurrentUser, ref pos))
            {
                // Create a Command context.
                var cmdContext = new SocketCommandContext(mClient, message);

                // Execute the command. (result does not indicate a return value, 
                // rather an object stating if the command executed successfully).
                await mCommandService.ExecuteAsync(cmdContext, pos, _serviceProvider);
            }

            await using var context = Context;
            DiscordChannel channel = context.DiscordChannels.FirstOrDefault(x => x.ChannelId == arg.Channel.Id);
            if (channel == null)
            {
                channel = new DiscordChannel
                {
                    ChannelId = arg.Channel.Id,
                    Name = arg.Channel.Name,
                    CreatedAt = UnixTimestamp.Timestamp(arg.Channel.CreatedAt.UtcDateTime)
                };
                if (!await context.SaveAsync(channel))
                    return;
            }
            else
            {
                if (!channel.Name.Equals(arg.Channel.Name))
                {
                    channel.Name = arg.Channel.Name;
                    await context.SaveAsync(channel);
                }
            }

            DiscordUser user = context.DiscordUsers.FirstOrDefault(x => x.DiscordUserId == arg.Author.Id);
            if (user == null)
            {
                user = new DiscordUser
                {
                    DiscordUserId = arg.Author.Id,
                    Name = arg.Author.Username,
                    Discriminator = arg.Author.Discriminator,
                    CreatedAt = UnixTimestamp.Now(),
                    AccountId = null,
                    AccountName = "",
                    GameUserId = null,
                    GameName = "",
                    CharactersSent = 0,
                    MessagesSent = 0
                };

                if (!await context.SaveAsync(user))
                    return;
            }
            else
            {
                if (!user.Name.Equals(arg.Author.Username) || !user.Discriminator.Equals(arg.Author.Discriminator))
                {
                    user.Name = arg.Author.Username;
                    user.Discriminator = arg.Author.Discriminator;
                    await context.SaveAsync(user);
                }
            }

            user.MessagesSent += 1;
            user.CharactersSent += (ulong)arg.Content.Length;
            await context.SaveAsync(user);

            await context.SaveAsync(new DiscordMessage
            {
                UserId = user.DiscordUserId,
                CurrentUserName = $"{user.Name}#{user.Discriminator}",
                Message = arg.Content,
                ChannelId = channel.ChannelId,
                Timestamp = UnixTimestamp.Now()
            });

            //await Log.WriteLogAsync(arg.Channel.Name, LogSeverity.Info,
            //    $"{user.Name}#{user.Discriminator} on Channel {arg.Channel.Name} >> {arg.Content}");
        }

        public async Task SendMessageAsync(string message, Embed embed = null)
        {
            if (!IsReady || mClient.GetChannel(MainChannelId) is not SocketTextChannel channel)
                return;
                        
            await channel.SendMessageAsync(message, false, embed);
        }

        public Task LogAsync(LogMessage log)
        {
            if (log.Exception != null)
                mLogger.LogCritical(log.Exception, log.Exception.Message);
            switch (log.Severity)
            {
                case LogSeverity.Error:
                case LogSeverity.Critical:
                    {
                        mLogger.LogError(log.Message);
                        break;
                    }
                case LogSeverity.Warning:
                    {
                        mLogger.LogWarning(log.Message);
                        break;
                    }
                case LogSeverity.Verbose:
                case LogSeverity.Debug:
                    {
                        mLogger.LogDebug(log.Message);
                        break;
                    }
                case LogSeverity.Info:
                default:
                    {
                        mLogger.LogInformation(log.Message);
                        break;
                    }
            }
            return Task.CompletedTask;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var task = ExecuteAsync(cancellationToken);
            return Task.CompletedTask;
        }

        private async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            mClient = new DiscordSocketClient();
            mClient.Log += LogAsync;

            mClient.Ready += ClientOnReadyAsync;

            mCommandService = new CommandService(new CommandServiceConfig
            {
                CaseSensitiveCommands = true,
                SeparatorChar = ' '
            });
            mCommandService.Log += LogAsync;

            await mCommandService.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);
            mClient.MessageReceived += OnCommandReceiveAsync;

            _serviceProvider = ConfigureServices();

            await mClient.LoginAsync(TokenType.Bot, mDiscordToken);
            await mClient.StartAsync();

            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000, cancellationToken);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return mClient.StopAsync();
        }
    }
}

using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace DiscoBot
{
    public class Program
    {
        private DiscordSocketClient _client;

        public static void Main(string[] args)
         => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            _client = new DiscordSocketClient();

            _client.Log += Log;

            await _client.LoginAsync(TokenType.Bot,
                /*My Token*/);

            await _client.StartAsync();

            _client.MessageReceived += MessageReceived;

            await Task.Delay(-1);
        }

        private async Task MessageReceived(SocketMessage message)
        {

            Random rand = new Random();
            var peeves = _client.CurrentUser.Mention;
            var member = message.Author.Mention;

            if (rand.Next(1, 101) <= 100 && !message.Author.IsBot)
            {
                string[] pranks = {
                    $"*{peeves} drops an apple on {member}'s head*",
                    $"*{member} trips on {peeves}' trap*",
                    $"*{peeves} ties {member}'s shoelaces*",
                    $"*{peeves} places a whoopee cushion under {member} as they sit*",
                    $"*{peeves} throws a pie at {member}'s face*",
                    $"*The bathroom floods as {peeves} teases Myrtle*",
                    $"*{peeves} locks the great hall*",
                    $"*{peeves} throws a stink bomb into the kitchen*",
                    $"*Drops super spicy sauce into {member}'s food*",
                    $"*Pulls {member}'s cap down over their eyes*",
                    $"*{peeves} throws a water ballon at {member}"
                };

                await (_client.GetChannel(433727090655756291) as ISocketMessageChannel).SendMessageAsync(pranks[rand.Next(pranks.Length)]);
            }

        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }

    public class Initialize
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;

        public Initialize(CommandService commands = null, DiscordSocketClient client = null)
        {
            _client = client ?? new DiscordSocketClient();
            _commands = commands ?? new CommandService();
        }

        public IServiceProvider BuildServiceProvider() => new ServiceCollection()
            .AddSingleton(_client)
            .AddSingleton(_commands)
            .AddSingleton<CommandHandler>()
            .BuildServiceProvider();
    }

    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;

        public CommandHandler(IServiceProvider services,DiscordSocketClient client, CommandService commands)
        {
            _commands = commands;
            _client = client;
            _services = services;
        }

        public async Task InstallCommandsAsync()
        {

            await _commands.AddModulesAsync(
                assembly: Assembly.GetEntryAssembly(),
                services: _services);

            _client.MessageReceived += HandleCommandAsync;
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            var message = messageParam as SocketUserMessage;
            if (message == null) return;

            int argPos = 0;

            if (!(message.HasCharPrefix('!', ref argPos) ||
                message.HasMentionPrefix(_client.CurrentUser, ref argPos)))
                return;

            var context = new SocketCommandContext(_client, message);

            var result = await _commands.ExecuteAsync(
                context: context,
                argPos: argPos,
                services: _services);
        }
    }

    public class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("talk")]
        public async Task TalkAsync([Remainder] string words)
        {
            await Context.Channel.SendMessageAsync(words);
        }
    }
}

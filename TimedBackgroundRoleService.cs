using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord.Rest;
using Discord.WebSocket;
using Discord;
using System.Text.RegularExpressions;
using Discord.Commands;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DoveRoleBot
{
    public class TimedBackgroundRoleService : IHostedService, IDisposable
    {
        private readonly ILogger<TimedBackgroundRoleService> _logger;
        private readonly BotSettings _settings;
        private DiscordRestClient _discordRestClient = null!;
        private DiscordSocketClient _discordSocketClient = null!;

        private Timer _timer = null!;
        private SocketGuild _guild = null!;
        private SocketGuildChannel _kickChannel = null!;


        //  https://discord.com/oauth2/authorize?client_id=913095241626693643&scope=bot+applications.commands&permissions=2050
        public TimedBackgroundRoleService(ILogger<TimedBackgroundRoleService> logger, IOptions<BotSettings> settings, DiscordRestClient discordRestClient, DiscordSocketClient discordSocketClient)
        {
            _logger = logger;
            _settings = settings.Value;
            _discordRestClient = discordRestClient;
            _discordSocketClient = discordSocketClient;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            ExecuteAsync().Wait();
            return Task.CompletedTask;
        }

        private async Task ExecuteAsync()
        {
            _logger.LogInformation("TimedBackgroundPriceService running.");

            await _discordRestClient.LoginAsync(TokenType.Bot, _settings.BotToken);
            await _discordSocketClient.LoginAsync(TokenType.Bot, _settings.BotToken);
            await _discordSocketClient.StartAsync();

            _discordSocketClient.Ready += _discordSocketClient_Ready;

            _timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromSeconds(_settings.UpdateInterval));
        }

        private Task _discordSocketClient_Ready()
        {
            _guild = _discordSocketClient.GetGuild(_settings.GuildId);
            _kickChannel = (SocketGuildChannel)_discordSocketClient.GetChannel(_settings.KickChannelId);

            return Task.CompletedTask;
        }
        private void DoWork(object? state)
        {
            KickNonMembers().GetAwaiter().GetResult();
        }

        private async Task KickNonMembers()
        {
            if (_guild != null)
            {
                var users = _guild.GetUsersAsync();

                await foreach (var result in _guild.GetUsersAsync())
                {
                    foreach (var user in result)
                    {
                        var isNotMember = !user.RoleIds.Any(x => x == _settings.RoleId);
                        var isOld = (DateTime.Now - user.JoinedAt) > TimeSpan.FromMinutes(_settings.TimeLimit);

                        if (isNotMember && isOld && !user.IsBot)
                        {
                            try
                            {
                                _logger.LogInformation($"Sending message to {user.Username} for not being verified");
                                await user.SendMessageAsync("You were kicked from Dove Finance for not being verified. Please rejoin at any time and verify. https://discord.gg/E3SgardR");
                            }
                            catch (Discord.Net.HttpException ex)
                            {
                                _logger.LogWarning(ex, $"{user.Username} is not accepting DMs.");
                            }

                            try
                            {
                                _logger.LogInformation($"Kicked {user.Username} for not verifying.");

                                var messageChannel = _kickChannel as ISocketMessageChannel;

                                await messageChannel!.SendMessageAsync($"{user.Mention} was kicked for not verifying in time.", allowedMentions: new AllowedMentions(AllowedMentionTypes.Users));

                                await user.KickAsync();
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, $"Error attempting to kick {user.Username}.");
                            }
                        }
                    }
                }
            }
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("TimedBackgroundPriceService is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}


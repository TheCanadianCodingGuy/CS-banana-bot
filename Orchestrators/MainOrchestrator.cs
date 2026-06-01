using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Numerics;
using System.Reflection;

namespace CS_banana_bot.Orchestrators;

public class MainOrchestrator : BackgroundService
{
    private readonly DiscordSocketClient _client;
    private readonly IConfiguration _configuration;
    private readonly InteractionService _interactionService;
    private readonly IServiceProvider _services;
    private readonly ILogger<MainOrchestrator> _logger;

    public MainOrchestrator(
        DiscordSocketClient client,
        IConfiguration configuration,
        InteractionService interactionService,
        IServiceProvider services,
        ILogger<MainOrchestrator> logger)
    {
        _client = client;
        _configuration = configuration;
        _interactionService = interactionService;
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _client.Log += LogAsync;
        _interactionService.Log += LogAsync;
        _client.Ready += ReadyAsync;
        _client.InteractionCreated += InteractionCreatedAsync;

        await _interactionService.AddModulesAsync(Assembly.GetExecutingAssembly(), _services);

        string? token = _configuration["BotSettings:Token"];
        if (string.IsNullOrWhiteSpace(token)) {
            _logger.LogCritical("Bot token is missing or empty in configuration!");
            return;
        }

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private Task LogAsync(LogMessage log)
    {
        LogLevel level = log.Severity switch
        {
            LogSeverity.Critical => LogLevel.Critical,
            LogSeverity.Error => LogLevel.Error,
            LogSeverity.Warning => LogLevel.Warning,
            LogSeverity.Info => LogLevel.Information,
            _ => LogLevel.Debug
        };

        _logger.Log(level, "{Message}", log.Message ?? log.Exception?.ToString());
        return Task.CompletedTask;
    }

    private async Task ReadyAsync()
    {
#if DEBUG
        string? guildId = _configuration["BotSettings:TestGuild"];
        if (string.IsNullOrWhiteSpace(guildId))
        {
            _logger.LogCritical("Test Guild ID is missing or empty in configuration!");
            return;
        }

        if (!ulong.TryParse(guildId, out ulong parsedGuildId))
        {
            _logger.LogCritical("Test Guild ID is not valid!");
            return;
        }

        await _interactionService.RegisterCommandsToGuildAsync(parsedGuildId);
#else
        await _interactionService.RegisterCommandsGloballyAsync();
#endif
        _logger.LogInformation("Successfully connected to Discord. Commands initialized.");
    }

    private async Task InteractionCreatedAsync(SocketInteraction interaction)
    {
        try
        {
            var context = new SocketInteractionContext(_client, interaction);
            await _interactionService.ExecuteCommandAsync(context, _services);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception caught during interaction routing.");

            if (interaction.Type == InteractionType.ApplicationCommand)
            {
                await interaction.GetOriginalResponseAsync().ContinueWith(async (msg) =>
                    await interaction.FollowupAsync("An execution error occurred inside the bot pipeline."));
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Disconnecting gracefully from Discord servers...");
        await _client.LogoutAsync();
        await _client.StopAsync();
        await base.StopAsync(cancellationToken);
    }
}
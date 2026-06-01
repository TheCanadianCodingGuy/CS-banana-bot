using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace CS_banana_bot.Infrastructure;

public class BotInitializer : BackgroundService
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _interactionService;
    private readonly IConfiguration _configuration;
    private readonly UserCommandsOrchestrator _userOrchestrator;
    private readonly IServiceProvider _services;
    private readonly ILogger<BotInitializer> _logger;

    public BotInitializer(
        DiscordSocketClient client,
        InteractionService interactionService,
        IConfiguration configuration,
        UserCommandsOrchestrator userOrchestrator,
        IServiceProvider services,
        ILogger<BotInitializer> logger)
    {
        _client = client;
        _interactionService = interactionService;
        _configuration = configuration;
        _userOrchestrator = userOrchestrator;
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _client.Ready += OnReadyAsync;
        _client.InteractionCreated += OnInteractionCreatedAsync;

        await _interactionService.AddModulesAsync(Assembly.GetExecutingAssembly(), _services);

        var token = _configuration["BotSettings:Token"];
        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogCritical("Bot token is missing or empty in configuration!");
            return;
        }
        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task OnReadyAsync()
    {
#if DEBUG
        var guildId = _configuration["BotSettings:TestGuild"];
        if (!string.IsNullOrWhiteSpace(guildId) && ulong.TryParse(guildId, out ulong parsedGuildId))
        {
            await _interactionService.RegisterCommandsToGuildAsync(parsedGuildId);
        }
        else
        {
            _logger.LogCritical("Test Guild ID is missing or invalid in development configuration!");
            return;
        }
#else
        await _interactionService.RegisterCommandsGloballyAsync();
#endif
        _logger.LogInformation("Discord bot initialized.");
    }

    private async Task OnInteractionCreatedAsync(SocketInteraction interaction)
    {
        await _userOrchestrator.OrchestrateInteractionAsync(interaction);
    }
}
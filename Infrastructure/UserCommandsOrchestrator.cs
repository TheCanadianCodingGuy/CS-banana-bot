using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace CS_banana_bot.Infrastructure;

public class UserCommandsOrchestrator
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _interactionService;
    private readonly IServiceProvider _services;
    private readonly ILogger<UserCommandsOrchestrator> _logger;

    public UserCommandsOrchestrator(
        DiscordSocketClient client,
        InteractionService interactionService,
        IServiceProvider services,
        ILogger<UserCommandsOrchestrator> logger)
    {
        _client = client;
        _interactionService = interactionService;
        _services = services;
        _logger = logger;
    }

    public async Task OrchestrateInteractionAsync(SocketInteraction interaction)
    {
        try
        {
            await _interactionService.ExecuteCommandAsync(
                new SocketInteractionContext(_client, interaction), 
                _services);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error orchestrating user interaction!");
            if (interaction.Type == InteractionType.ApplicationCommand && !interaction.HasResponded)
            {
                await interaction.RespondAsync("An unknown error occurred executing this command!");
            }
        }
    }
}
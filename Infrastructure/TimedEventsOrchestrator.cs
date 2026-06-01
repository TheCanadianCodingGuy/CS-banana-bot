using CS_banana_bot.Modules;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CS_banana_bot.Infrastructure;

public class TimedEventOrchestrator : BackgroundService
{
    private readonly DiscordSocketClient _client;
    private readonly TestTimedModule _testTimedModule;
    private readonly ILogger<TimedEventOrchestrator> _logger;

    public TimedEventOrchestrator(
        DiscordSocketClient client,
        TestTimedModule testTimedModule,
        ILogger<TimedEventOrchestrator> logger)
    {
        _client = client;
        _testTimedModule = testTimedModule;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer minuteTimer = new(TimeSpan.FromMinutes(1));

        while (await minuteTimer.WaitForNextTickAsync(stoppingToken))
        {
            if (_client.ConnectionState != ConnectionState.Connected) continue;
            _logger.LogInformation("Timed Event Test Fired.");
            //usually switch to different module functions.
            await _testTimedModule.HandleTimedTest();
        }
    }
}

using CS_banana_bot.Orchestrators;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateDefaultBuilder(args);

var socketConfig = new DiscordSocketConfig
{
    GatewayIntents = GatewayIntents.Guilds,
    LogGatewayIntentWarnings = false
};

builder.ConfigureServices((hostContext, services) =>
{
    services.AddSingleton(socketConfig);
    services.AddSingleton<DiscordSocketClient>();
    services.AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()));

    services.AddHostedService<MainOrchestrator>();
});

var host = builder.Build();
await host.RunAsync();
using CS_banana_bot.BusinessLogic.Interfaces;
using CS_banana_bot.BusinessLogic.Test;
using CS_banana_bot.Infrastructure;
using CS_banana_bot.Modules;
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

    //Modules
    services.AddScoped<TestModule>();
    services.AddSingleton<TestTimedModule>();

    //Business Logic
    services.AddSingleton<IGetTestData, GetTestData>();

    //Lifecycle
    services.AddSingleton<UserCommandsOrchestrator>();
    services.AddHostedService<BotInitializer>();
    services.AddHostedService<TimedEventOrchestrator>();
});

var host = builder.Build();
await host.RunAsync();
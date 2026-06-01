using CS_banana_bot.BusinessLogic.Interfaces;
using CS_banana_bot.Formatters;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace CS_banana_bot.Modules
{
    public class TestTimedModule(IGetTestData test, IConfiguration configuration, DiscordSocketClient client)
    {
        private readonly IGetTestData _test = test;
        private readonly IConfiguration _configuration = configuration;
        private readonly DiscordSocketClient _client = client;

        public async Task HandleTimedTest()
        {
            string formatted = await ResponseFormatter.FormatHandleTimedTest(_test.GetTimedTestString());
            await SendTimedMessage(formatted);
        }

        public async Task SendTimedMessage(string text)
        {
            var channelIdString = _configuration["BotSettings:TestChannel"];
            if (!ulong.TryParse(channelIdString, out ulong channelId))
            {
                throw new InvalidOperationException($"Configuration key TestChannel is missing or invalid.");
            }

            var channel = _client.GetChannel(channelId) as Discord.ITextChannel ?? throw new InvalidOperationException($"Channel {channelId} not found or is not a text channel.");
            await channel.SendMessageAsync(text);
        }
    }
}

using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Text;

namespace CS_banana_bot.Modules
{
    public class TestModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("test", "Test to see if the connection pipeline is active.")]
        public async Task HandleTestCommand()
        {
            await RespondAsync("Hello World!");
        }
    }
}

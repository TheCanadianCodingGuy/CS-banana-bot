using CS_banana_bot.BusinessLogic.Interfaces;
using CS_banana_bot.Formatters;
using Discord.Interactions;

namespace CS_banana_bot.Modules;

public class TestModule(IGetTestData test) : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IGetTestData _test = test;

    [SlashCommand("test2", "Hello world, again!")]
    public async Task HandleTest2()
    {
       var response = await ResponseFormatter.FormatHandleTest(_test.GetTestString());
       await RespondAsync(response);
    }
}
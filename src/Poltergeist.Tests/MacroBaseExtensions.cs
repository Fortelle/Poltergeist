using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Tests;

public static class MacroBaseExtensions
{
    public static ProcessorResult Test(this MacroBase macro, MacroProcessorArguments? arguments = null)
    {
        return MacroProcessor.Execute(macro, arguments);
    }

    public static async Task<ProcessorResult> TestAsync(this MacroBase macro, MacroProcessorArguments? arguments = null)
    {
        return await MacroProcessor.ExecuteAsync(macro);
    }

    public static void AssertSuccess(this ProcessorResult result)
    {
        if (result.Exception is not null)
        {
            throw result.Exception;
        }
        Assert.IsTrue(result.IsSucceeded);
    }

    public static void AssertOutput(this MacroBase macro, Func<IReadOnlyParameterValueCollection, bool> func)
    {
        var result = MacroProcessor.Execute(macro);
        if (result.Exception is not null)
        {
            throw result.Exception;
        }

        Assert.IsTrue(func(result.Output));
    }

    public static async Task AssertOutputAsync(this MacroBase macro, Func<IReadOnlyParameterValueCollection, bool> func)
    {
        var result = await MacroProcessor.ExecuteAsync(macro);
        if (result.Exception is not null)
        {
            throw result.Exception;
        }

        Assert.IsTrue(func(result.Output));
    }
}

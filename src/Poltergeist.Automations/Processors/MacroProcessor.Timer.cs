using System.Diagnostics;

namespace Poltergeist.Automations.Processors;

public partial class MacroProcessor : IDisposable
{
    private readonly Stopwatch Timer = new();

    public TimeSpan GetElapsedTime()
    {
        return Timer.Elapsed;
    }
}

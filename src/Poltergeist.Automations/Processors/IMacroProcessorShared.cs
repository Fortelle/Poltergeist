using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace Poltergeist.Automations.Processors;

public interface IMacroProcessorShared : IMacroProcessorInformation
{
    bool IsCancellationRequested { get; }
    CancellationToken CancellationToken { get; }
    void ThrowIfCancellationRequested();

    T GetService<T>() where T : class;
    object GetService(Type type);
    bool TryGetService<T>(out T? service);
    bool TryGetService(Type type, out object? service);

    TimeSpan GetElapsedTime();

    Task Pause(PauseReason reason);

    void Resume();

    void ReportComment(string comment);
}

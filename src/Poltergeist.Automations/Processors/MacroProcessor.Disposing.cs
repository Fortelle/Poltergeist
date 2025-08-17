namespace Poltergeist.Automations.Processors;

public partial class MacroProcessor : IDisposable
{
    private bool IsDisposed;

    private void Dispose(bool disposing)
    {
        if (IsDisposed)
        {
            return;
        }

        if (disposing)
        {
            ServiceProvider?.Dispose();
            ServiceProvider = null;

            foreach (var value in SessionStorage.Values)
            {
                if (value is IDisposable idis)
                {
                    idis.Dispose();
                }
            }
        }

        IsDisposed = true;
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

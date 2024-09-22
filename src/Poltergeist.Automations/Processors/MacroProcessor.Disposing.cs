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

            foreach (var item in SessionStorage)
            {
                if (item.Value is IDisposable idis)
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

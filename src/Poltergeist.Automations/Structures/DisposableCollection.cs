namespace Poltergeist.Automations.Structures;

public class DisposableCollection<T> : List<T>, IDisposable where T : IDisposable
{
    protected bool IsDisposed;

    public DisposableCollection() : base()
    {
    }

    public DisposableCollection(int capacity) : base(capacity)
    {
    }

    public DisposableCollection(IEnumerable<T> collection) : base(collection)
    {
    }

    protected virtual void Dispose(bool disposing)
    {
        if (IsDisposed)
        {
            return;
        }

        if (disposing)
        {
            foreach (var item in this.AsEnumerable().Reverse())
            {
                item.Dispose();
            }
            Clear();
        }

        IsDisposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}

namespace Poltergeist.Common.Structures;

public class DisposableList<T> : List<T>, IDisposable where T : IDisposable
{
    public void Dispose()
    {
        foreach (var item in this)
        {
            item?.Dispose();
        }
    }
}

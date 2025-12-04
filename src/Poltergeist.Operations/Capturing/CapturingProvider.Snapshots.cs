using System.Drawing;

namespace Poltergeist.Operations.Capturing;

public partial class CapturingProvider
{
    public string? CurrentSnapshotKey { get; private set; }

    public bool IsUsingSnapshot => string.IsNullOrEmpty(CurrentSnapshotKey);

    private Dictionary<string, Bitmap> CachedSnapshots = new();

    public string TakeSnapshot()
    {
        var snapshotKey = Guid.NewGuid().ToString();
        TakeSnapshot(snapshotKey);
        return snapshotKey;
    }

    public void TakeSnapshot(string snapshotKey)
    {
        ReleaseSnapshot(snapshotKey);

        var snapshotImage = Capture();
        CachedSnapshots.Add(snapshotKey, snapshotImage);

        Logger.Debug($"Cached snapshot \"{snapshotKey}\".");
    }

    public string TakeAndSwitchToSnapshot()
    {
        var snapshotName = TakeSnapshot();
        SwitchToSnapshot(snapshotName);
        return snapshotName;
    }

    public void TakeAndSwitchToSnapshot(string snapshotKey)
    {
        TakeSnapshot(snapshotKey);
        SwitchToSnapshot(snapshotKey);
    }

    public void ReleaseSnapshot(string snapshotKey)
    {
        if (!CachedSnapshots.TryGetValue(snapshotKey, out var bmp))
        {
            return;
        }

        bmp.Dispose();
        CachedSnapshots.Remove(snapshotKey);

        Logger.Debug($"Released the cached snapshot \"{snapshotKey}\".");

        if (CurrentSnapshotKey == snapshotKey)
        {
            SwitchToLive();
        }
    }

    public void ReleaseCurrentSnapshot()
    {
        if (string.IsNullOrEmpty(CurrentSnapshotKey))
        {
            return;
        }

        ReleaseSnapshot(CurrentSnapshotKey);
    }

    public void ReleaseAllSnapshots()
    {
        foreach (var bmp in CachedSnapshots.Values)
        {
            bmp.Dispose();
        }
        CachedSnapshots.Clear();
    }

    public void SwitchToSnapshot(string snapshotKey)
    {
        CurrentSnapshotKey = snapshotKey;
        Logger.Debug($"Switched the capturing source to the snapshot \"{snapshotKey}\".");
    }

    public void SwitchToLive()
    {
        CurrentSnapshotKey = null;
        Logger.Debug($"Switched the capturing source to live.");
    }

    public Bitmap GetSnapshot(string snapshotKey)
    {
        if (!CachedSnapshots.TryGetValue(snapshotKey, out var snapshot))
        {
            throw new Exception($"The specified snapshot key \"{snapshotKey}\"does not exist in the cache.");
        }

        return snapshot;
    }
}

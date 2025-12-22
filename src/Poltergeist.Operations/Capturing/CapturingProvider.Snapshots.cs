using System.Drawing;

namespace Poltergeist.Operations.Capturing;

public partial class CapturingProvider
{
    public string? CurrentSnapshotKey { get; private set; }

    public bool IsUsingSnapshot => string.IsNullOrEmpty(CurrentSnapshotKey);

    private Dictionary<string, Bitmap> CachedSnapshots = new();

    public string TakeSnapshot()
    {
        return TakeSnapshotInternal(null, false);
    }

    public void TakeSnapshot(string snapshotKey)
    {
        TakeSnapshotInternal(snapshotKey, false);
    }

    public string TakeAndSwitchToSnapshot()
    {
        return TakeSnapshotInternal(null, true);
    }

    public void TakeAndSwitchToSnapshot(string snapshotKey)
    {
        TakeSnapshotInternal(snapshotKey, true);
    }

    private string TakeSnapshotInternal(string? snapshotKey, bool switchto)
    {
        if (string.IsNullOrEmpty(snapshotKey))
        {
            snapshotKey = Guid.NewGuid().ToString();
        }
        else if (CachedSnapshots.TryGetValue(snapshotKey, out var oldSnapshot))
        {
            oldSnapshot.Dispose();
            CachedSnapshots.Remove(snapshotKey);

            Logger.Debug($"Released the old snapshot \"{snapshotKey}\".");
        }

        var snapshotImage = Capture(new CapturingOptions() { IgnoresSnapshot = true });
        CachedSnapshots.Add(snapshotKey, snapshotImage);

        Logger.Debug($"Cached snapshot \"{snapshotKey}\".");

        if (switchto)
        {
            SwitchToSnapshot(snapshotKey);
        }

        return snapshotKey;
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

    public void ReleaseSnapshot()
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

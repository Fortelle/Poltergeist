namespace Poltergeist.Helpers;

internal static class SingleInstanceHelper
{
    private const string MutexKey = $"Poltergeist_Single_Instance_Mutex";

    private static readonly Mutex mutex = new(true, MutexKey);

    public static bool IsFirstInstance() => mutex.WaitOne(TimeSpan.Zero);

    public static void Close() => mutex.Close();
}

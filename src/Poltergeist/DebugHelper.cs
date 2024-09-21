namespace Poltergeist;

#if DEBUG
internal static class DebugHelper
{
    public static async void Do()
    {
        await Task.CompletedTask;
    }
}
#endif
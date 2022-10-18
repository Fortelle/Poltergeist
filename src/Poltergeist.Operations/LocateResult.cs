namespace Poltergeist.Operations
{
    public enum LocateResult
    {
        None,

        Succeeded,

        NotFound,
        Minimized,
        WrongRectangle,
        SizeNotMatch,
        RescaleOverflow,

        EmptyParameters,
    }
}

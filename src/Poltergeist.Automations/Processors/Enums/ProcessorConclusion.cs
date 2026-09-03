namespace Poltergeist.Automations.Processors;

/// <summary>
/// Specifies the result of a completed processor.
/// </summary>
public enum ProcessorConclusion
{
    /// <summary>
    /// The cause of the end was not specified.
    /// </summary>
    Unknown,

    /// <summary>
    /// The processor was completed successfully.
    /// </summary>
    Success,

    /// <summary>
    /// The processor was completed with a failure.
    /// </summary>
    Failure,

    /// <summary>
    /// The processor was canceled by the user.
    /// </summary>
    Canceled,

    /// <summary>
    /// An error occurred during the execution of the processor.
    /// </summary>
    ErrorOccurred,

    /// <summary>
    /// The processor was terminated forcefully by the user.
    /// </summary>
    Terminated,

    /// <summary>
    /// An unhandled fatal error occurred during the execution of the processor.
    /// </summary>
    Crushed,
}

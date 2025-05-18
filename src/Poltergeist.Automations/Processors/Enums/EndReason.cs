namespace Poltergeist.Automations.Processors;

/// <summary>
/// Specifies the reason for the end of a processor.
/// </summary>
public enum EndReason
{
    /// <summary>
    /// The cause of the end was not specified.
    /// </summary>
    Unknown,

    /// <summary>
    /// The processor was completed successfully.
    /// </summary>
    Complete,

    /// <summary>
    /// The processor was interrupted by the user.
    /// </summary>
    Interrupted,

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

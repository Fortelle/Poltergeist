namespace Poltergeist.Automations.Processors;

public enum ProcessorStatus
{
    /// <summary>
    /// The processor is ready to run.
    /// </summary>
    Idle,

    /// <summary>
    /// The processor is not able to run due to an initialization error.
    /// </summary>
    Invalid,

    /// <summary>
    /// The processor is launching.
    /// </summary>
    Launching,

    /// <summary>
    /// Indicates that the processor has launched.
    /// </summary>
    Launched,

    /// <summary>
    /// The processor is running.
    /// </summary>
    Running,

    /// <summary>
    /// The processor has crushed due to an unexpected error.
    /// </summary>
    Crushed,

    /// <summary>
    /// The processor is faulting.
    /// </summary>
    Faulting,

    /// <summary>
    /// The processor has stopped due to an error.
    /// </summary>
    Faulted,

    /// <summary>
    /// The processor has completed correctly.
    /// </summary>
    Complete,

    /// <summary>
    /// The processor is paused.
    /// </summary>
    Paused,

    /// <summary>
    /// The processor is stopping.
    /// </summary>
    Stopping,

    /// <summary>
    /// The processor has stopped.
    /// </summary>
    Stopped,

    /// <summary>
    /// The processor is terminating.
    /// </summary>
    Terminating,

    /// <summary>
    /// The processor has been terminated forcefully.
    /// </summary>
    Terminated,
}

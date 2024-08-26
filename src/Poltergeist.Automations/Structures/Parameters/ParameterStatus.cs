namespace Poltergeist.Automations.Structures.Parameters;

public enum ParameterStatus
{
    Normal,
    ReadOnly, // can be seen but can not be edited
    Hidden,   // can not be seen
    Experimental, // shows an experimental icon
    DevelopmentOnly, // can only be seen in development mode
    Deprecated, // always returns the default value
}

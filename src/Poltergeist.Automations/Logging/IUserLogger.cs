namespace Poltergeist.Automations.Logging;

public interface IUserLogger
{
    public void Trace(string message);
    public void Debug(string message);
    public void Info(string message);
    public void Warn(string message);
    public void Error(string message);
    public void Critical(string message);
}

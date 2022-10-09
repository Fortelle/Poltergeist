using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace Poltergeist.Automations.Logging;

public interface IServiceLogger
{
    public void Trace(string message);
    public void Trace(object variable, [CallerArgumentExpression("variable")] string name = null);
    public void Debug(string message);
    public void Debug(string message, params object[] variables);
    public void Info(string message);
    public void Warn(string message);
    public void Error(string message);
    public void Critical(string message);
}

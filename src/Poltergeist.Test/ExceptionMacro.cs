using Poltergeist.Automations.Components.Repeats;
using Poltergeist.Automations.Exceptions;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Components.Loops;

namespace Poltergeist.Test;

public class ExceptionMacro : RepeatableMacro
{
    public ExceptionMacro(string name) : base(name)
    {
        UserOptions.Add("On initialization", false);
        UserOptions.Add("On work begin", false);
        UserOptions.Add("On work end", false);
        UserOptions.Add("On loop begin", false);
        UserOptions.Add("On loop iteration", false);
        UserOptions.Add("On loop checknext", false);
        UserOptions.Add("On loop end", false);

        Begin = _ =>
        {
            if (UserOptions.Get<bool>("On loop begin"))
            {
                throw new MacroRunningException("This exception is thrown on loop begin.");
            }
        };
        End = _ =>
        {
            if (UserOptions.Get<bool>("On loop end"))
            {
                throw new MacroRunningException("This exception is thrown on loop end.");
            }
        };
        Iteration = _ =>
        {
            if (UserOptions.Get<bool>("On loop iteration"))
            {
                throw new MacroRunningException("This exception is thrown on loop iteration.");
            }
        };
        CheckNext = _ =>
        {
            if (UserOptions.Get<bool>("On loop checknext"))
            {
                throw new MacroRunningException("This exception is thrown on loop checknext.");
            }
        };
    }

    protected override void OnConfigure(MacroServiceCollection services, IConfigureProcessor processor)
    {
        base.OnConfigure(services, processor);

        if (UserOptions.Get<bool>("On initialization"))
        {
            throw new MacroRunningException("This exception is thrown on initialization.");
        }
    }

    protected override void OnProcess(MacroProcessor processor)
    {
        base.OnProcess(processor);

        var work = processor.GetService<WorkingService>();

        if (UserOptions.Get<bool>("On work begin"))
        {
            work.Beginning += (s, e) =>
            {
                throw new MacroRunningException("This exception is thrown on availability check.");
            };
        }

        if (UserOptions.Get<bool>("On work end"))
        {
            work.Beginning += (s, e) =>
            {
                throw new MacroRunningException("This exception is thrown on work end.");
            };
        }
    }

}

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
            return true;
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
            return IterationResult.Continue;
        };
        CheckNext = _ =>
        {
            if (UserOptions.Get<bool>("On loop checknext"))
            {
                throw new MacroRunningException("This exception is thrown on loop checknext.");
            }
            return CheckNextResult.Continue;
        };
    }

    protected override void ReadyProc(MacroProcessor processor)
    {
        base.ReadyProc(processor);

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

    protected override void InitProc()
    {
        base.InitProc();

        if (UserOptions.Get<bool>("On initialization"))
        {
            throw new MacroRunningException("This is an exception thrown on initialization.");
        }

    }

    protected override void ConfigureProc(MacroServiceCollection services)
    {
        if (UserOptions.Get<bool>("On initialization"))
        {
            throw new MacroRunningException("This is an exception thrown on initialization.");
        }
    }

}

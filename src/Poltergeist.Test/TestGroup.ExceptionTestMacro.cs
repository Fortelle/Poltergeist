using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Automations.Attributes;
using Poltergeist.Automations.Components.Loops;
using Poltergeist.Automations.Exceptions;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Test;

public partial class TestGroup
{
    [AutoLoad]
    public class ExceptionTestMacro : LoopMacro
    {
        public ExceptionTestMacro() : base("test_exception")
        {
            Title = "Exception Test";
            Description = "This macro throws exceptions in different stages.";

            UserOptions.Add("In processor initialization", false);
            UserOptions.Add("In workflow beginning", false);
            UserOptions.Add("In workflow ending", false);
            UserOptions.Add("In loop begining", false);
            UserOptions.Add("In loop iteration", false);
            UserOptions.Add("In loop ending", false);

            Before = _ =>
            {
                if (UserOptions.Get<bool>("In loop begining"))
                {
                    throw new MacroRunningException("This exception is thrown in the beginning of the loop.");
                }
            };
            After = _ =>
            {
                if (UserOptions.Get<bool>("In loop ending"))
                {
                    throw new MacroRunningException("This exception is thrown in the ending of the loop.");
                }
            };
            Execution = _ =>
            {
                if (UserOptions.Get<bool>("In loop iteration"))
                {
                    throw new MacroRunningException("This exception is thrown in the iteration of the loop.");
                }
            };
        }

        protected override void OnConfigure(ServiceCollection services, IConfigureProcessor processor)
        {
            base.OnConfigure(services, processor);

            if (UserOptions.Get<bool>("In processor initialization"))
            {
                throw new MacroRunningException("This exception is thrown in the initialization of the processor.");
            }
        }

        protected override void OnProcess(MacroProcessor processor)
        {
            base.OnProcess(processor);

            var work = processor.GetService<WorkingService>();

            if (UserOptions.Get<bool>("In workflow beginning"))
            {
                work.Beginning += (s, e) =>
                {
                    throw new MacroRunningException("This exception is thrown in the beginning of the workflow.");
                };
            }

            if (UserOptions.Get<bool>("In workflow ending"))
            {
                work.Ending += (s, e) =>
                {
                    throw new MacroRunningException("This exception is thrown in the ending of the workflow.");
                };
            }
        }

    }

}
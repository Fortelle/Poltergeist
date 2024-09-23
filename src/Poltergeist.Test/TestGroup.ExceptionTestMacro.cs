using Poltergeist.Automations.Attributes;
using Poltergeist.Automations.Components.Loops;
using Poltergeist.Automations.Macros;
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
            IsSingleton = true;

            UserOptions.Add($"{nameof(MacroProcessor)}.{nameof(IBackMacro.Configure)}", false);
            UserOptions.Add($"{nameof(MacroProcessor)}.{nameof(IBackMacro.Prepare)}", false);
            UserOptions.Add($"{nameof(ProcessorCheckStartHook)}", false);
            UserOptions.Add($"{nameof(IterationStartedHook)}", false);
            UserOptions.Add($"{nameof(LoopMacro)}.{nameof(LoopMacro.Before)}", false);
            UserOptions.Add($"{nameof(LoopMacro)}.{nameof(LoopMacro.Execute)}", false);
            UserOptions.Add($"{nameof(LoopMacro)}.{nameof(LoopMacro.After)}", false);

            Before = args =>
            {
                if (args.Processor.Options.Get<bool>($"{nameof(LoopMacro)}.{nameof(LoopMacro.Before)}"))
                {
                    throw new MacroRunningException("This exception is thrown in the beginning of the loop.");
                }
            };
            After = args =>
            {
                if (args.Processor.Options.Get<bool>($"{nameof(LoopMacro)}.{nameof(LoopMacro.After)}"))
                {
                    throw new MacroRunningException("This exception is thrown in the ending of the loop.");
                }
            };
            Execute = args =>
            {
                Thread.Sleep(1000);
                if (args.Processor.Options.Get<bool>($"{nameof(LoopMacro)}.{nameof(LoopMacro.Execute)}"))
                {
                    throw new MacroRunningException("This exception is thrown in the iteration of the loop.");
                }
            };
        }

        protected override void OnConfigure(IConfigurableProcessor processor)
        {
            base.OnConfigure(processor);

            if (processor.Options.Get<bool>($"{nameof(MacroProcessor)}.{nameof(IBackMacro.Configure)}"))
            {
                throw new MacroRunningException("This exception is thrown in the configuration of the processor.");
            }
        }

        protected override void OnPrepare(IPreparableProcessor processor)
        {
            base.OnPrepare(processor);

            if (processor.Options.Get<bool>($"{nameof(ProcessorCheckStartHook)}"))
            {
                processor.Hooks.Register<ProcessorCheckStartHook>(hook =>
                {
                    throw new MacroRunningException($"This exception is thrown by <{nameof(ProcessorCheckStartHook)}>.");
                });
            }

            if (processor.Options.Get<bool>($"{nameof(IterationStartedHook)}"))
            {
                processor.Hooks.Register<ProcessorStartedHook>(hook =>
                {
                    throw new MacroRunningException($"This exception is thrown by <{nameof(ProcessorStartedHook)}>.");
                });
            }

            if (processor.Options.Get<bool>($"{nameof(MacroProcessor)}.{nameof(IBackMacro.Prepare)}"))
            {
                throw new MacroRunningException("This exception is thrown in the preparation of the processor.");
            }
        }
    }

}
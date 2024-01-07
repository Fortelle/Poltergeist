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

            UserOptions.Add($"{nameof(MacroProcessor)}.{nameof(IMacroBase.OnConfigure)}", false);
            UserOptions.Add($"{nameof(MacroProcessor)}.{nameof(IMacroBase.OnPrepare)}", false);
            UserOptions.Add($"{nameof(ProcessorCheckStartHook)}", false);
            UserOptions.Add($"{nameof(IterationStartedHook)}", false);
            UserOptions.Add($"{nameof(LoopMacro)}.{nameof(LoopMacro.Before)}", false);
            UserOptions.Add($"{nameof(LoopMacro)}.{nameof(LoopMacro.Execute)}", false);
            UserOptions.Add($"{nameof(LoopMacro)}.{nameof(LoopMacro.After)}", false);

            Before = _ =>
            {
                if (UserOptions.Get<bool>($"{nameof(LoopMacro)}.{nameof(LoopMacro.Before)}"))
                {
                    throw new MacroRunningException("This exception is thrown in the beginning of the loop.");
                }
            };
            After = _ =>
            {
                if (UserOptions.Get<bool>($"{nameof(LoopMacro)}.{nameof(LoopMacro.After)}"))
                {
                    throw new MacroRunningException("This exception is thrown in the ending of the loop.");
                }
            };
            Execute = _ =>
            {
                Thread.Sleep(1000);
                if (UserOptions.Get<bool>($"{nameof(LoopMacro)}.{nameof(LoopMacro.Execute)}"))
                {
                    throw new MacroRunningException("This exception is thrown in the iteration of the loop.");
                }
            };
        }

        protected override void OnConfigure(IConfigurableProcessor processor)
        {
            base.OnConfigure(processor);

            if (UserOptions.Get<bool>($"{nameof(MacroProcessor)}.{nameof(IMacroBase.OnConfigure)}"))
            {
                throw new MacroRunningException("This exception is thrown in the configuration of the processor.");
            }
        }

        protected override void OnPrepare(IPreparableProcessor processor)
        {
            base.OnPrepare(processor);

            if (UserOptions.Get<bool>($"{nameof(ProcessorCheckStartHook)}"))
            {
                processor.Hooks.Register<ProcessorCheckStartHook>(() =>
                {
                    throw new MacroRunningException($"This exception is thrown by <{nameof(ProcessorCheckStartHook)}>.");
                });
            }

            if (UserOptions.Get<bool>($"{nameof(IterationStartedHook)}"))
            {
                processor.Hooks.Register<ProcessorStartedHook>(() =>
                {
                    throw new MacroRunningException($"This exception is thrown by <{nameof(ProcessorStartedHook)}>.");
                });
            }

            if (UserOptions.Get<bool>($"{nameof(MacroProcessor)}.{nameof(IMacroBase.OnPrepare)}"))
            {
                throw new MacroRunningException("This exception is thrown in the preparation of the processor.");
            }
        }
    }

}
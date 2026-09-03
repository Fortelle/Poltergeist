using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Macros.Loops;

namespace Poltergeist.Automations.Processors;

public static class WorkflowControllerExtensions
{
    extension(WorkflowController con)
    {
        public void Report(ProgressInstrumentInfo info, int index)
        {
            var hook = new UpdateInstrumentInfoHook()
            {
                Index = index,
                Info = info,
            };
            con.Processor.GetService<HookService>().Raise(hook);
        }

        public void Sleep(int timeout)
        {
            con.Processor.CancellationToken.WaitHandle.WaitOne(timeout);
            con.Processor.ThrowIfCancellationRequested();
        }
    }
}

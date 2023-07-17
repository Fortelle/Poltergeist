using Poltergeist.Automations.Attributes;
using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Automations.Components.Loops;
using Poltergeist.Automations.Macros;

namespace Poltergeist.Test;

public partial class ExampleGroup : MacroGroup
{

    [AutoLoad]
    public LoopMacro ToastNotificationExample = new("example_toastnotification")
    {
        Title = "Toast Notification Example",
        Description = "This example shows how to send toast notifications to windows.",

        LoopOptions =
        {
            DefaultCount = 3,
            Instrument = LoopInstrumentType.List,
        },

        Execution = (args) =>
        {
            Thread.Sleep(15000);
        },

        CheckContinue = (args) =>
        {
            args.Processor.GetService<InteractionService>().Show(new ToastModel()
            {
                Text = $"Iteration {args.IterationIndex + 1} completed",
            });
        },

        After = (args) =>
        {
            args.Processor.GetService<InteractionService>().Show(new ToastModel()
            {
                Text = $"Loop completed",
            });
        },
    };
}

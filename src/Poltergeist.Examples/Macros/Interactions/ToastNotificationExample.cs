using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Automations.Macros.Oneshots;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Examples.Macros;

[ExampleMacro]
public class ToastModelExample : CommonOneshotMacroBase
{
    public ToastModelExample() : base()
    {
        Title = nameof(ToastModel);

        Category = "Interactions";

        Description = "This example shows how to push a toast notification to Windows.";
    }

    protected override void OnExecute(WorkflowController controller)
    {
        var toastModel = new ToastModel()
        {
            Text = $"This is an example message.",
        };

        var interactionService = controller.Processor.GetService<InteractionService>();
        interactionService.Push(toastModel);
    }
}

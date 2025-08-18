using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Automations.Macros;

namespace Poltergeist.Examples.Macros;

[ExampleMacro]
public class ToastModelExample : BasicMacro
{
    public ToastModelExample() : base()
    {
        Title = nameof(ToastModel);

        Category = "Interactions";

        Description = "This example shows how to push a toast notification to Windows.";

        Execute = (args) =>
        {
            var toastModel = new ToastModel()
            {
                Text = $"This is an example message.",
            };

            var interactionService = args.Processor.GetService<InteractionService>();
            interactionService.Push(toastModel);
        };
    }
}

using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Macros.Oneshots;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Examples.Macros;

[ExampleMacro]
public class DialogExample : CommonOneshotMacroBase
{
    public DialogExample() : base()
    {
        Title = "Dialog Example";

        Category = "Use Cases";

        Description = "This example shows how to interact with the user by popping up dialogs during a process.";
    }

    protected async override Task OnExecuteAsync(WorkflowController controller)
    {
        const int totalTasks = 5;
        const int totalSteps = 10;
        const int errorStep = 5;

        var interactionService = controller.Processor.GetService<InteractionService>();
        var instrument = controller.Processor.GetService<DashboardService>().Create<ProgressListInstrument>(inst =>
        {
            inst.Title = "Steps:";
        });

        for (var i = 0; i < totalTasks; i++)
        {
            instrument.Add(new(ProgressStatus.Idle)
            {
                Text = $"Step {i + 1}"
            });
        }

        for (var i = 0; i < totalTasks; i++)
        {
            instrument.Update(i, new(ProgressStatus.Busy)
            {
                Progress = 0,
            });

            for (var j = 0; j < totalSteps; j++)
            {
                await Task.Delay(100);

                instrument.Update(i, new()
                {
                    Progress = 1.0 * j / totalSteps,
                });

                if (j == errorStep)
                {
                    var dialogModel = new DialogModel()
                    {
                        Text = "An intentional error occurred. What do you want to do?",
                        PrimaryButtonText = "Ignore",
                        SecondaryButtonText = "Skip",
                        CloseButtonText = "Stop",
                    };
                    await interactionService.Interact(dialogModel);

                    if (dialogModel.Result == DialogResult.Primary)
                    {
                        continue;
                    }
                    else if (dialogModel.Result == DialogResult.Secondary)
                    {
                        instrument.Update(i, new(ProgressStatus.Warning)
                        {
                            Subtext = "Skipped",
                        });
                        break;
                    }
                    else
                    {
                        instrument.Update(i, new(ProgressStatus.Failure)
                        {
                            Subtext = "Stopped",
                        });
                        return;
                    }
                }

                if (j == totalSteps - 1)
                {
                    instrument.Update(i, new(ProgressStatus.Success)
                    {
                        Progress = 1,
                        Subtext = "Done",
                    });
                }
            }
        }

    }
}

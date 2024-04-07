using Poltergeist.Automations.Attributes;
using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Macros;

namespace Poltergeist.Test;

public partial class ExampleGroup
{

    [AutoLoad]
    public BasicMacro DialogExample = new("example_dialog")
    {
        Title = "Dialog Example",
        Description = "This example shows how to create a dialog.",

        IsSingleton = true,

        ExecuteAsync = async (args) =>
        {
            var li = args.Processor.GetService<DashboardService>().Create<ProgressListInstrument>(inst =>
            {
                inst.Title = "Steps:";
            });

            for (var i = 0; i < 5; i++)
            {
                li.Update(i, new(ProgressStatus.Busy)
                {
                    Text = $"Step {i + 1}",
                    Subtext = "Processing",
                    Progress = 0,
                });

                var isError = false;
                for (var j = 0; j < 10; j++)
                {
                    li.Update(i, new()
                    {
                        Progress = j / 10d,
                    });

                    isError = i % 2 == 1 && j == 5;
                    if (isError)
                    {
                        break;
                    }

                    await Task.Delay(100);
                }

                if (isError)
                {
                    var interactionService = args.Processor.GetService<InteractionService>();
                    var dialog = new DialogModel()
                    {
                        Text = "An error occurred while processing the task.",
                        PrimaryButtonText = "Retry",
                        SecondaryButtonText = "Ignore",
                        CloseButtonText = "Abort",
                    };

                    await interactionService.ShowAsync(dialog);

                    if (dialog.Result == DialogResult.Primary)
                    {
                        i--;
                        continue;
                    }
                    else if (dialog.Result == DialogResult.Secondary)
                    {
                        li.Update(i, new(ProgressStatus.Failure)
                        {
                            Subtext = "Error",
                        });
                    }
                    else
                    {
                        li.Update(i, new(ProgressStatus.Failure)
                        {
                            Subtext = "Error",
                        });
                        return;
                    }
                }
                else
                {
                    li.Update(i, new(ProgressStatus.Success)
                    {
                        Subtext = "Done",
                        Progress = 1,
                    });
                }
            }

        }

    };

}

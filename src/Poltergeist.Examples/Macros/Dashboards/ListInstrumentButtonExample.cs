using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Examples.Macros;

[ExampleMacro]
public class ListInstrumentButtonExample : BasicMacro
{
    public ListInstrumentButtonExample() : base()
    {
        Title = "ListInstrument buttons";

        Category = "Instruments";

        Description = "This example shows how to interact with the user through the ListInstrument.";

        OptionDefinitions.Add(new OptionDefinition<bool>("countdown"));

        ExecuteAsync = async (args) =>
        {
            var li = args.Processor.GetService<ProgressListInstrument>();
            li.Title = "Tasks:";

            var instrumentService = args.Processor.GetService<DashboardService>();
            instrumentService.Add(li);

            var loop = true;
            var i = 0;
            var useCountdown = args.Processor.Options.GetValueOrDefault<bool>("countdown");

            while (loop)
            {
                for (var j = 0; j < 8; j++)
                {
                    li.Update(i, new(ProgressStatus.Busy)
                    {
                        Progress = (j + 1) / 10d,
                        Text = $"Processing...",
                    });

                    await Task.Delay(100);
                }

                li.Update(i, new(ProgressStatus.Warning)
                {
                    Text = $"Oops! Something went wrong.",
                    Buttons = [
                        new ListInstrumentButton(){
                            Text = "Retry",
                            Callback = () =>
                            {
                                args.Processor.Resume();
                            },
                        },
                        new ListInstrumentButton(){
                            Text = "Continue",
                            CountdownSeconds = useCountdown ? 15 : 0,
                            Callback = () =>
                            {
                                li.Override(i, new(ProgressStatus.Success)
                                {
                                    Text = $"You chose 'continue'.",
                                    Subtext = "Continue",
                                });

                                i++;

                                args.Processor.Resume();
                            },
                        },
                        new ListInstrumentButton(){
                            Text = "Stop",
                            Callback = () =>
                            {
                                li.Override(i, new(ProgressStatus.Failure)
                                {
                                    Text = $"You chose 'Stop'.",
                                    Subtext = "Stop",
                                });

                                loop = false;

                                args.Processor.Resume();
                            },
                        },
                    ],
                });

                await args.Processor.Pause(Automations.Processors.PauseReason.WaitForInput);

                if (!loop)
                {
                    break;
                }

                if (i >= 10)
                {
                    break;
                }
            }
        };

    }
}
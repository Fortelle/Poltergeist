using Poltergeist.Automations.Components.Journals;
using Poltergeist.Automations.Macros;

namespace Poltergeist.Examples.Macros;

[ExampleMacro]
public class JournalServiceExample : BasicMacro
{
    public JournalServiceExample() : base()
    {
        Title = nameof(JournalService);

        Category = "Instruments";

        Description = "This example shows how to use the JournalService.";

        Execute = (args) =>
        {
            var journalService = args.Processor.GetService<JournalService>();

            for (var i = 1; i <= 5; i++)
            {
                journalService.AppendLine($"{new string('#', i)} Header {i}");
            }

            journalService.AppendLine($"## List:");
            for (var j = 0; j < 3; j++)
            {
                journalService.AppendLine($"* Item {j + 1}");
            }
            for (var j = 0; j < 3; j++)
            {
                journalService.AppendLine($"** Item {j + 1}");
            }
            for (var j = 0; j < 3; j++)
            {
                journalService.AppendLine($"*** Item {j + 1}");
            }

            journalService.AppendLine($"## Quote:");
            for (var j = 0; j < 3; j++)
            {
                journalService.AppendLine($"> Item {j + 1}");
            }

            journalService.AppendLine($"## Styling text:");
            journalService.AppendLine($"This is **bold** text");
            journalService.AppendLine($"This text is _italicized_");
            journalService.AppendLine($"This was ~~mistaken~~ text");
            journalService.AppendLine($"***All this text is important***");
            journalService.AppendLine($"This is a <sub>subscript</sub> text");
            journalService.AppendLine($"This is a <sup>superscript</sup> text");
            journalService.AppendLine($"This is an <ins>underlined</ins> text");
            journalService.AppendLine($"---");

        };
    }

}

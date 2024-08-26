using CommunityToolkit.Mvvm.ComponentModel;

namespace Poltergeist.Pages.Macros.Instruments;

public partial class ButtonViewModel : ObservableRecipient
{
    public string? Key { get; init; }

    public string? BaseText { get; init; }

    [ObservableProperty]
    private string? _text;

    public string? Argument { get; init; }

    public int Countdown { get; set; }
}

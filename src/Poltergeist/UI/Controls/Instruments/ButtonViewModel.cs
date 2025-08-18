using CommunityToolkit.Mvvm.ComponentModel;

namespace Poltergeist.UI.Controls.Instruments;

public partial class ButtonViewModel : ObservableRecipient
{
    public string? Key { get; init; }

    public string? BaseText { get; init; }

    [ObservableProperty]
    public partial string? Text { get; set; }

    public string? Argument { get; init; }

    public int Countdown { get; set; }
}

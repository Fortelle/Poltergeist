﻿namespace Poltergeist.Pages.Macros.Instruments;

public class PanelViewModel
{
    public required string Key { get; init; }

    public required string Header { get; init; }

    public required InstrumentsWrapper Instruments { get; init; }
}

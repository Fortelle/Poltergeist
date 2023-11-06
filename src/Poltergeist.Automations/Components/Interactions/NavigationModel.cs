﻿namespace Poltergeist.Automations.Components.Interactions;

public class NavigationModel : InteractionModel
{
    public string PageKey { get; set; }
    public object? Argumment { get; set; }

    public NavigationModel(string pageKey)
    {
        PageKey = pageKey;
    }
}

﻿using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Contracts.Services;
using Poltergeist.Helpers;
using Poltergeist.Pages;
using Poltergeist.Services;
using Windows.UI.ViewManagement;

namespace Poltergeist;

public sealed partial class MainWindow : WindowEx
{
    private readonly Microsoft.UI.Dispatching.DispatcherQueue dispatcherQueue;

    private readonly UISettings settings;

    public MainWindow()
    {
        // https://github.com/microsoft/microsoft-ui-xaml/issues/7164#issuecomment-1171618502
        _ = Dispatcher;

        InitializeComponent();

        AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Poltergeist/Assets/WindowIcon.ico"));
        Content = null;
        Content = new ProgressRing()
        {
            IsActive = true,
            Background = new SolidColorBrush(Colors.LightGray),
        };

        //Title = "AppDisplayName".GetLocalized();
        Title = "Poltergeist";
        CenterToScreen();
        AppWindow.Closing += AppWindow_Closing;

        // Theme change code picked from https://github.com/microsoft/WinUI-Gallery/pull/1239
        dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
        settings = new UISettings();
        settings.ColorValuesChanged += Settings_ColorValuesChanged; // cannot use FrameworkElement.ActualThemeChanged event
    }

    private void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
    {
        if (App.GetService<MacroManager>().IsBusy)
        {
            TipService.Show(new TipModel()
            {
                Type = TipType.Disabled,
                Title = "One or more macros are running.",
            });
            args.Cancel = true;
            return;
        }

        var tabview = App.GetService<INavigationService>().TabView;
        if(tabview is not null)
        {
            foreach(var tabviewitem in tabview.TabItems.OfType<TabViewItem>())
            {
                if(tabviewitem.Content is not IApplicationClosing closing)
                {
                    continue;
                }

                closing.OnApplicationClosing();
            }
        }

        App.GetService<LocalSettingsService>().Save();
        App.GetService<MacroManager>().GlobalOptions.Save();
    }

    // this handles updating the caption button colors correctly when windows system theme is changed
    // while the app is open
    private void Settings_ColorValuesChanged(UISettings sender, object args)
    {
        // This calls comes off-thread, hence we will need to dispatch it to current app's thread
        dispatcherQueue.TryEnqueue(() =>
        {
            TitleBarHelper.ApplySystemThemeToCaptionButtons();
        });
    }

    private void CenterToScreen()
    {
        var displayArea = DisplayArea.GetFromWindowId(AppWindow.Id, DisplayAreaFallback.Nearest);
        if (displayArea is not null)
        {
            var x = (int)((displayArea.WorkArea.Width - Width) / 2);
            var y = (int)((displayArea.WorkArea.Height - Height) / 2);
            AppWindow.Move(new(x, y));
        }
        
    }

}

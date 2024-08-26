using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Contracts.Services;
using Poltergeist.Services;

namespace Poltergeist.Notifications;

public class AppNotificationService 
{
    private bool IsInitialized { get; set; }

    public AppNotificationService()
    {
    }

    ~AppNotificationService()
    {
        Unregister();
    }

    public void Initialize()
    {
        if (IsInitialized)
        {
            return;
        }

        AppNotificationManager.Default.NotificationInvoked += OnNotificationInvoked;
        AppNotificationManager.Default.Register();

        IsInitialized = true;
    }

    private static void OnNotificationInvoked(AppNotificationManager sender, AppNotificationActivatedEventArgs args)
    {
        App.MainWindow.DispatcherQueue.TryEnqueue(() =>
        {
            if (args.Arguments.TryGetValue("macro", out var value))
            {
                App.GetService<INavigationService>().NavigateTo("macro:" + value);
            }

            var msg = new InteractionMessage(args.Argument);
            App.GetService<MacroManager>().SendMessage(msg);

            App.MainWindow.BringToFront();
        });
    }

    private static void Unregister()
    {
        AppNotificationManager.Default.Unregister();
    }

    public void Show(ToastModel model)
    {
        var builder = CreateBuilder(model);

        Show(builder);
    }

    private static AppNotificationBuilder CreateBuilder(ToastModel model)
    {
        var builder = new AppNotificationBuilder()
            .AddArgument("conversationId", model.Id)
            .AddArgument(InteractionMessage.MacroKeyName, model.ShellKey)
            .AddArgument(InteractionMessage.ProcessIdName, model.ProcessId)
            ;

        var macro = App.GetService<MacroManager>().GetShell(model.ShellKey!);

        builder.AddText(model.Title ?? macro?.Title ?? model.ShellKey);

        if (model.Text is not null)
        {
            builder.AddText(model.Text);
        }
        if (model.ImageUri is not null)
        {
            builder.SetInlineImage(model.ImageUri);
        }

        return builder;
    }

    private bool Show(AppNotificationBuilder builder)
    {
        Initialize();

        var appNotification = builder.BuildNotification();

        AppNotificationManager.Default.Show(appNotification);

        return appNotification.Id != 0;
    }

}

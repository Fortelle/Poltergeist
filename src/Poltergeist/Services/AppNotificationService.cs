using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Contracts.Services;
using Poltergeist.Services;

namespace Poltergeist.Notifications;

public class AppNotificationService 
{
    private bool Initialized { get; set; }

    public AppNotificationService()
    {
    }

    ~AppNotificationService()
    {
        Unregister();
    }

    public void Initialize()
    {
        if (Initialized) return;

        AppNotificationManager.Default.NotificationInvoked += OnNotificationInvoked;
        AppNotificationManager.Default.Register();

        Initialized = true;
    }

    public void OnNotificationInvoked(AppNotificationManager sender, AppNotificationActivatedEventArgs args)
    {
        App.MainWindow.DispatcherQueue.TryEnqueue(() =>
        {
            if (args.Arguments.ContainsKey("macro"))
            {
                App.GetService<INavigationService>().NavigateTo("macro:" + args.Arguments["macro"]);
            }

            var msg = new InteractionMessage(args.Argument);
            App.GetService<MacroManager>().SendMessage(msg);

            App.MainWindow.BringToFront();
        });
    }

    public void Unregister()
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
            .AddArgument(InteractionMessage.MacroKeyName, model.MacroKey)
            .AddArgument(InteractionMessage.ProcessIdName, model.ProcessId)
            ;

        var macro = App.GetService<MacroManager>().GetMacro(model.MacroKey);

        builder.AddText(model.Title ?? macro?.Title ?? model.MacroKey);

        if (model.Text != null)
        {
            builder.AddText(model.Text);
        }
        if (model.ImageUri != null)
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

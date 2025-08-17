using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Modules.Macros;

namespace Poltergeist.Modules.Interactions;

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
        PoltergeistApplication.TryEnqueue(() =>
        {
            if (args.Arguments.TryGetValue("macro", out var value))
            {
                var pageKey = MacroManager.GetPageKey(value);
                PoltergeistApplication.GetService<MacroManager>().OpenPage(pageKey);
            }

            var msg = new InteractionMessage(args.Argument);
            PoltergeistApplication.GetService<MacroManager>().SendMessage(msg);

            PoltergeistApplication.Current.MainWindow.BringToFront();
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
            .AddArgument(InteractionMessage.ProcessorIdName, model.ProcessorId)
            ;

        builder.AddText(model.Title);

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

using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Modules.App;
using Poltergeist.Modules.Events;

namespace Poltergeist.Modules.Interactions;

public class AppNotificationService
{
    public const string ConversationIdKey = "conversation_id";

    private bool IsInitialized { get; set; }

    private readonly Dictionary<string, Action<IDictionary<string, string>>> Handlers = new();

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

    private void OnNotificationInvoked(AppNotificationManager sender, AppNotificationActivatedEventArgs args)
    {
        if (args.Arguments.TryGetValue(ConversationIdKey, out var conversationId) && Handlers.TryGetValue(conversationId, out var action))
        {
            action.Invoke(args.Arguments);
            Handlers.Remove(conversationId);
        }

        PoltergeistApplication.GetService<AppEventService>().Publish(new AppNotificationReceivedEvent()
        {
            Arguments = args.Arguments
        });

        PoltergeistApplication.Current.MainWindow.BringToFront();
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

    public bool Show(AppNotificationBuilder builder)
    {
        Initialize();

        var appNotification = builder.BuildNotification();

        AppNotificationManager.Default.Show(appNotification);

        return appNotification.Id != 0;
    }

    public bool Show(AppNotificationBuilder builder, string conversationId, Action<IDictionary<string, string>> handler)
    {
        Initialize();

        Handlers.Add(conversationId, handler);

        var appNotification = builder.BuildNotification();

        AppNotificationManager.Default.Show(appNotification);

        return appNotification.Id != 0;
    }

    private static AppNotificationBuilder CreateBuilder(ToastModel model)
    {
        var builder = new AppNotificationBuilder()
            .AddArgument(InteractionMessage.ProcessorIdKey, model.ProcessorId)
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
}

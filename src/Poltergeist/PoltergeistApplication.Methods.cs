using Microsoft.UI.Dispatching;
using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Helpers;
using Poltergeist.Modules.Interactions;

namespace Poltergeist;

public partial class PoltergeistApplication
{

	public static T GetService<T>() where T : class
	{
		var service = GetService(typeof(T));
		return (T)service!;
	}

	public static object GetService(Type type)
    {
        if (Current.Host is null)
        {
            throw new ArgumentException($"{nameof(PoltergeistApplication)}.{nameof(Host)} is not ready yet.");
        }

        var service = Current.Host?.Services.GetService(type);
		if (service is null)
		{
			throw new ArgumentException($"{type} needs to be registered in ConfigureServices within App.xaml.cs.");
		}
		return service;
	}

	public static void ShowTeachingTip(string message)
    {
        TipService.Show(new TipModel()
		{
			Text = message,
		});
	}

	public static void ShowException(Exception exception)
    {
        TipService.Show(new TipModel()
		{
			Type = TipType.Error,
			Text = exception.Message,
		});
	}

	public static string Localize(string resourceKey, params object?[] args)
	{
		return ResourceHelper.Localize(resourceKey, args);
	}

	public static void TryEnqueue(DispatcherQueueHandler callback)
	{
        if (App.Current is null)
        {
            return;
        }

        if (App.Current.State >= ApplicationState.Exiting)
        {
            return;
        }

        App.Current.DispatcherQueue.TryEnqueue(callback);
	}

}

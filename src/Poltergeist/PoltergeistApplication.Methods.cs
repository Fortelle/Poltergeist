using Microsoft.UI.Dispatching;
using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Automations.Utilities;
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
		var service = CurrentPoltergeist.Host?.Services.GetService(type);
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
		MainWindow.DispatcherQueue.TryEnqueue(callback);
	}

}

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Microsoft.UI.Dispatching;

namespace Poltergeist.Models;

public class SynchronizableCollection<TModel, TViewModel> : ObservableCollection<TViewModel?> where TViewModel : class
{
    private DispatcherQueue DispatcherQueue { get; }
    private Func<TModel?, TViewModel?> ToViewModel { get; }
    public bool KeepsNull { get; set; }

    public SynchronizableCollection(ObservableCollection<TModel> modelCollection, Func<TModel?, TViewModel?> func, DispatcherQueue dispatcherQueue)
    {
        DispatcherQueue = dispatcherQueue;
        ToViewModel = func;

        for (var i = 0; i < modelCollection.Count; i++)
        {
            var model = modelCollection[i];
            Add(CreateViewModel(model));
        }

        modelCollection.CollectionChanged += SynchronizableCollectionChanged;
    }

    public void SynchronizableCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    for (var i = 0; i < e.NewItems!.Count; i++)
                    {
                        Insert(i + e.NewStartingIndex, CreateViewModel((TModel?)e.NewItems![i]));
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    for (var i = 0; i < e.OldItems!.Count; i++)
                    {
                        if (this[e.OldStartingIndex] is IDisposable disposable)
                        {
                            disposable.Dispose();
                        }
                        RemoveItem(e.OldStartingIndex);
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    for (var i = 0; i < e.OldItems!.Count; i++)
                    {
                        if (this[i + e.OldStartingIndex] is IDisposable disposable)
                        {
                            disposable.Dispose();
                        }
                        SetItem(i + e.OldStartingIndex, CreateViewModel((TModel?)e.NewItems![i]));
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    throw new NotImplementedException();
                case NotifyCollectionChangedAction.Reset:
                    Clear();
                    for (var i = 0; i < e.NewItems!.Count; i++)
                    {
                        Add(CreateViewModel((TModel?)e.NewItems![i]));
                    }
                    break;
            }
        });
    }

    private TViewModel? CreateViewModel(TModel? model)
    {
        if (model is null && KeepsNull)
        {
            return null;
        }
        else
        {
            return ToViewModel?.Invoke(model) ?? null;
        }
    }
}

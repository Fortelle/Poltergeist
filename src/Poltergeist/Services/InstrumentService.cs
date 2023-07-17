using Microsoft.UI.Xaml.Controls;
using Poltergeist.Automations.Components.Panels;
using Poltergeist.Pages.Macros.Instruments;

namespace Poltergeist.Services;

public class InstrumentManager
{
    public List<InstrumentInfo> Informations { get; } = new();

    public InstrumentManager()
    {
    }

    public void AddInfo<TModel, TView, TViewModel>()
        where TModel : IInstrumentModel
        where TView : UserControl
        where TViewModel : IInstrumentViewModel
    {
        Informations.Add(new()
        {
            ModelType = typeof(TModel),
            ViewType = typeof(TView),
            ViewModelType = typeof(TViewModel),
        });
    }

    public InstrumentInfo GetInfo(IInstrumentModel model)
    {
        var type = model.GetType();
        var info = Informations.FirstOrDefault(x => x.ModelType.IsAssignableFrom(type));
        if (info is null)
        {
            throw new ArgumentOutOfRangeException(nameof(model));
        }

        return info;
    }

    public InstrumentInfo GetInfo(IInstrumentViewModel viewmodel)
    {
        var type = viewmodel.GetType();
        var info = Informations.FirstOrDefault(x => x.ViewModelType.IsAssignableFrom(type));
        if (info is null)
        {
            throw new ArgumentOutOfRangeException(nameof(viewmodel));
        }

        return info;
    }


    public class InstrumentInfo
    {
        public required Type ModelType { get; init; }
        public required Type ViewType { get; init; }
        public required Type ViewModelType { get; init; }
    }

}

using System.Collections.ObjectModel;
using System.Windows.Media;

namespace Poltergeist.Automations.Instruments;

public class ImageInstrumentViewModel : InstrumentItemViewModel
{
    public int Max { get; set; }

    public ObservableCollection<ImageSource> Images { get; set; } = new();

    public ImageInstrumentViewModel(ImageInstrument ii)
    {
        Max = ii.Max;
        ii.Changed += OnChanged;
    }

    private void OnChanged(int index, ImageSource image)
    {
        if (index == -1)
        {
            if (Max > 0 && Images.Count == Max)
            {
                Images.RemoveAt(0);
            }
            Images.Add(image);
        }
        else
        {
            Images[index] = image;
        }
    }
}

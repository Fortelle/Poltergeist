using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;

namespace Poltergeist.Automations.Components;

public class DialogService : MacroService
{
    public DialogService(MacroProcessor processor) : base(processor)
    {
    }

    public void ShowDialog<T>(params object[] args) where T : Page
    {
        var page = Activator.CreateInstance(typeof(T), args) as Page;
        var width = page.Width;
        var height = page.Height;

        var frm = new Window();
        frm.Content = page;
        frm.Width = width;
        frm.Height = height;

        frm.ShowDialog();
    }

    public async Task ShowDialogAsync<T>(params object[] args) where T : Page
    {
        Processor.RaiseAction(() =>
        {
            var page = Activator.CreateInstance(typeof(T), args) as Page;
            var width = page.Width;
            var height = page.Height;

            var frm = new Window();
            frm.Content = page;
            frm.Closed += (x, y) =>
            {
                Processor.Resume();
            };

            frm.Activate();
        });

        await Processor.Pause();
    }

    public void Dispose()
    {
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Macros;
using Poltergeist.Helpers;
using Poltergeist.Modules.Macros;
using Windows.UI;
using Windows.UI.Text;

namespace Poltergeist.UI.Pages.Macros;

public partial class TerminalActionViewModel : ObservableRecipient
{
    public TerminalAction Quest { get; }

    public MacroInstance Instance { get; }

    public RichTextBlock? RichTextBlock { get; set; }

    [ObservableProperty]
    public partial double ProgressValue { get; set; }

    [ObservableProperty]
    public partial string? ProgressText { get; set; }

    [ObservableProperty]
    public partial bool IsIndeterminate { get; set; }

    [ObservableProperty]
    public partial bool IsRunning { get; set; }

    public string Title { get; }

    private CancellationTokenSource? CTS;

    public TerminalActionViewModel(TerminalAction action, MacroInstance instance)
    {
        Quest = action;
        Instance = instance;
        
        Title = Quest.Text + " - " + (instance.Template!.Title ?? instance.Template!.Key);
    }

    [RelayCommand]
    public async Task Start()
    {
        IsRunning = true;
        var actionService = PoltergeistApplication.GetService<MacroActionService>();
        var context = new TerminalActionContext()
        {
            Macro = Instance.Template!,
            Options = actionService.GetOptions(Instance),
            Environments = actionService.GetEnvironments(Instance),
        };

        RichTextBlock!.Blocks.Clear();

        context.Logged += OnLogged;
        context.ProgressChanged += OnProgressChanged;
        CTS = new();
        context.CancellationToken = CTS.Token;

        try
        {
            await Quest.ExecuteAsync(context);
        }
        catch (Exception exception)
        {
            PushLine(exception);
        }

        CTS.Dispose();
        CTS = null;

        context.Logged -= OnLogged;
        context.ProgressChanged -= OnProgressChanged;
        IsRunning = false;
    }

    [RelayCommand]
    public void Cancel()
    {
        CTS?.Cancel();
    }

    private void OnLogged(string message)
    {
        PushLine(new TextLine(message));
    }

    private void OnProgressChanged(double value, string? message = null)
    {
        ProgressValue = value;
        ProgressText = message;
        IsIndeterminate = value == -1;
    }

    private void PushLine(Exception exception)
    {
        PushLine(new TextLine(exception.Message));
        if (exception.InnerException is not null)
        {
            PushLine(exception.InnerException);
        }
    }

    private void PushLine(TextLine line)
    {
        if (RichTextBlock is null)
        {
            throw new InvalidOperationException();
        }

        var run = new Run
        {
            Text = line.Text
        };
        if (line.Foreground.HasValue)
        {
            run.Foreground = GetBrush(ColorUtil.ToColor(line.Foreground.Value));
        }
        if (line.IsBold.HasValue && line.IsBold == true)
        {
            run.FontWeight = FontWeights.Bold;
        }
        if (line.IsItalic.HasValue && line.IsItalic == true)
        {
            run.FontStyle = FontStyle.Italic;
        }

        var block = new Paragraph();
        block.Inlines.Add(run);

        RichTextBlock.Blocks.Add(block);
    }

    private readonly Dictionary<Color, SolidColorBrush> Brushes = new();

    private SolidColorBrush GetBrush(Color color)
    {
        if (!Brushes.TryGetValue(color, out var brush))
        {
            brush = new SolidColorBrush(color);
            Brushes.Add(color, brush);
        }

        return brush;
    }

}

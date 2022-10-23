using System;
using System.Drawing;
using System.Windows;
using Poltergeist.Services;
using Poltergeist.Views;

namespace Poltergeist
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var rect = App.GetSettings<Rectangle>("app.windowposition");
            if (rect != default)
            {
                this.Top = rect.Top;
                this.Left = rect.Left;
                this.Width = rect.Width;
                this.Height = rect.Height;
            }

            this.Content = App.GetService<ShellPage>();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            SingletonHelper.Load(this);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var rect = new Rectangle((int)Top, (int)Left, (int)Width, (int)Height);
            var localSettings = App.GetService<LocalSettingsService>();
            localSettings.SetSetting("app.windowposition", rect);
            localSettings.Save();
        }

    }
}

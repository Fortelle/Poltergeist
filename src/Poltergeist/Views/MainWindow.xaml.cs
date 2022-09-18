using Poltergeist.ViewModels;
using Poltergeist.Views;
using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

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

            var rect = App.GetSettings<Rectangle>("WindowPosition");
            if (rect != default)
            {
                this.Top = rect.Top;
                this.Left = rect.Left;
                this.Width = rect.Width;
                this.Height = rect.Height;
            }

            this.Content = App.GetService<ShellPage>();
        }

        private void NavigationViewControl_ItemInvoked(ModernWpf.Controls.NavigationView sender, ModernWpf.Controls.NavigationViewItemInvokedEventArgs args)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var rect = new Rectangle((int)Top, (int)Left, (int)Width, (int)Height);
            App.SetSettings("WindowPosition", rect);
        }
    }
}

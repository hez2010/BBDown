using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using BBDown.Ava.Services;

namespace BBDown.Ava.Pages
{
    public partial class HomePage : UserControl
    {
        private readonly ProcessingIndicator processingIndicator;
        public HomePage()
        {
            InitializeComponent();
            processingIndicator = AvaloniaLocator.Current.GetRequiredService<ProcessingIndicator>();
        }

        private async void Download_OnClick(object? sender, RoutedEventArgs args)
        {
            processingIndicator.IsProcessing = true;
            await Task.Delay(1000);
            processingIndicator.IsProcessing = false;
        }
    }
}

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using BBDown.Ava.ViewModels;

namespace BBDown.Ava.Pages;

public partial class HomePage : UserControl
{
    private readonly MainWindowViewModel mainWindowViewModel;
    public HomePage()
    {
        InitializeComponent();
        mainWindowViewModel = AvaloniaLocator.Current.GetRequiredService<MainWindowViewModel>();
    }

    private async void Download_OnClick(object? sender, RoutedEventArgs args)
    {
        mainWindowViewModel.ShowProgressRing = true;
        try
        {

        }
        finally
        {
            mainWindowViewModel.ShowProgressRing = false;
        }
    }
}

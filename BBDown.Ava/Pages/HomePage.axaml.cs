using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using BBDown.Ava.ViewModels;

namespace BBDown.Ava.Pages;

public partial class HomePage : UserControl
{
    private readonly MainViewModel mainViewModel;
    public HomePage()
    {
        InitializeComponent();
        mainViewModel = AvaloniaLocator.Current.GetRequiredService<MainViewModel>();
    }

    private async void Download_OnClick(object? sender, RoutedEventArgs args)
    {
        mainViewModel.ShowProgressRing = true;
        try
        {

        }
        finally
        {
            mainViewModel.ShowProgressRing = false;
        }
    }
}

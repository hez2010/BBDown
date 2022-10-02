using Avalonia;
using Avalonia.Controls;
using BBDown.Ava.Pages;
using BBDown.Ava.Services;
using BBDown.Ava.ViewModels;
using FluentAvalonia.UI.Controls;
using Markdown.Avalonia;

namespace BBDown.Ava;

public partial class MainView : UserControl
{
    private readonly MainViewModel mainViewModel;

    public MainView()
    {
        AvaloniaLocator.CurrentMutable
            .Bind<MainViewModel>().ToSingleton<MainViewModel>()
            .Bind<UpdateService>().ToSingleton<UpdateService>();

        mainViewModel = AvaloniaLocator.Current.GetRequiredService<MainViewModel>();

        InitializeComponent();
        
        naviView.SelectedItem = naviView.MenuItems.Cast<NavigationViewItem>().First();
        DataContext = mainViewModel;
        mainViewModel.DialogInvoked += async viewModel =>
        {
            var dialog = new ContentDialog
            {
                Title = viewModel.DialogTitle,
                Content = new MarkdownScrollViewer { Markdown = viewModel.DialogContent },
                PrimaryButtonText = viewModel.DialogPrimaryButtonText,
                SecondaryButtonText = viewModel.DialogSecondaryButtonText,
                CloseButtonText = viewModel.DialogCloseButtonText,
            };
            return await dialog.ShowAsync();
        };
    }
    
    private Type? GetContentType(string? tag)
    {
        return tag switch
        {
            "Home" => typeof(HomePage),
            "Settings" => typeof(SettingsPage),
            _ => null
        };
    }

    private void NaviView_OnSelectionChanged(object? sender, NavigationViewSelectionChangedEventArgs args)
    {
        var selectedTag = args.IsSettingsSelected
            ? "Settings"
            : args.SelectedItem is NavigationViewItem { Tag: string tag }
                ? tag
                : null;
        var type = GetContentType(selectedTag);
        if (type is null) return;
        contentFrame.NavigateToType(type, null, new() { IsNavigationStackEnabled = true });
    }

}
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using BBDown.Ava.Pages;
using BBDown.Ava.Services;
using BBDown.Ava.ViewModels;
using FluentAvalonia.Styling;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media;
using Markdown.Avalonia;
using System.Runtime.InteropServices;

namespace BBDown.Ava;

public partial class MainWindow : CoreWindow
{
    private readonly MainWindowViewModel mainWindowViewModel = AvaloniaLocator.Current.GetRequiredService<MainWindowViewModel>();
    private readonly FluentAvaloniaTheme theme = AvaloniaLocator.Current.GetRequiredService<FluentAvaloniaTheme>();

    public MainWindow()
    {
        InitializeComponent();
        naviView.SelectedItem = naviView.MenuItems.Cast<NavigationViewItem>().First();
        DataContext = mainWindowViewModel;
        mainWindowViewModel.DialogInvoked += async viewModel =>
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

    #region Mica

    private void OnRequestedThemeChanged(FluentAvaloniaTheme sender, RequestedThemeChangedEventArgs args)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            if (IsWindows11 && args.NewTheme != FluentAvaloniaTheme.HighContrastModeString)
            {
                TryEnableMicaEffect(sender);
            }
            else if (args.NewTheme == FluentAvaloniaTheme.HighContrastModeString)
            {
                TransparencyLevelHint = WindowTransparencyLevel.None;
                SetValue(BackgroundProperty, AvaloniaProperty.UnsetValue);
            }
        }
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        theme.RequestedThemeChanged += OnRequestedThemeChanged;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            if (IsWindows11 && theme.RequestedTheme != FluentAvaloniaTheme.HighContrastModeString)
            {
                TryEnableMicaEffect(theme);
            }
        }

        theme.ForceWin32WindowToTheme(this);
    }

    private void TryEnableMicaEffect(FluentAvaloniaTheme theme)
    {
        TransparencyBackgroundFallback = Brushes.Transparent;
        TransparencyLevelHint = WindowTransparencyLevel.Mica;

        // The background colors for the Mica brush are still based around SolidBackgroundFillColorBase resource
        // BUT since we can't control the actual Mica brush color, we have to use the window background to create
        // the same effect. However, we can't use SolidBackgroundFillColorBase directly since its opaque, and if
        // we set the opacity the color become lighter than we want. So we take the normal color, darken it and 
        // apply the opacity until we get the roughly the correct color
        // NOTE that the effect still doesn't look right, but it suffices. Ideally we need access to the Mica
        // CompositionBrush to properly change the color but I don't know if we can do that or not
        if (theme.RequestedTheme == FluentAvaloniaTheme.DarkModeString)
        {
            var color = this.TryFindResource("SolidBackgroundFillColorBase", out var value) ? (Color2)(Color)value : new Color2(32, 32, 32);
            color = color.LightenPercent(-0.8f);
            Background = new ImmutableSolidColorBrush(color, 0.78);
        }
        else if (theme.RequestedTheme == FluentAvaloniaTheme.LightModeString)
        {
            var color = this.TryFindResource("SolidBackgroundFillColorBase", out var value) ? (Color2)(Color)value : new Color2(243, 243, 243);
            color = color.LightenPercent(0.5f);
            Background = new ImmutableSolidColorBrush(color, 0.9);
        }
    }

    #endregion

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

    private async void Window_Loaded(object? sender, RoutedEventArgs args)
    {
        await AvaloniaLocator.Current.GetRequiredService<UpdateService>().CheckUpdateAsync();
    }
}

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using BBDown.Ava.Pages;
using BBDown.Ava.Services;
using FluentAvalonia.Styling;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media;
using System.Runtime.InteropServices;

namespace BBDown.Ava;

public partial class MainWindow : CoreWindow
{
    private readonly ProcessingIndicator processingIndicator;
    public MainWindow()
    {
        InitializeComponent();
        naviView.SelectedItem = naviView.MenuItems.Cast<NavigationViewItem>().First();
        processingIndicator = AvaloniaLocator.Current.GetRequiredService<ProcessingIndicator>();
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
                SetValue(BackgroundProperty, AvaloniaProperty.UnsetValue);
            }
        }
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        var theme = AvaloniaLocator.Current.GetRequiredService<FluentAvaloniaTheme>();
        theme.RequestedThemeChanged += OnRequestedThemeChanged;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            if (IsWindows11 && theme.RequestedTheme != FluentAvaloniaTheme.HighContrastModeString)
            {
                TransparencyBackgroundFallback = Brushes.Transparent;
                TransparencyLevelHint = WindowTransparencyLevel.Mica;
                TryEnableMicaEffect(theme);
            }
        }

        theme.ForceWin32WindowToTheme(this);
    }

    private void TryEnableMicaEffect(FluentAvaloniaTheme theme)
    {
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

    private void Window_OnLoaded(object? sender, RoutedEventArgs args)
    {
        processingIndicator.PropertyChanged += 
            (obj, args) 
                => progressRing.IsVisible = obj is ProcessingIndicator { IsProcessing: var p } && p;
    }
}

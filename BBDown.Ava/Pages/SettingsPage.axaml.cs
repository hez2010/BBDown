using Avalonia;
using Avalonia.Controls;
using FluentAvalonia.Styling;

namespace BBDown.Ava.Pages;

public partial class SettingsPage : UserControl
{
    private readonly FluentAvaloniaTheme theme = AvaloniaLocator.Current.GetRequiredService<FluentAvaloniaTheme>();

    public SettingsPage()
    {
        InitializeComponent();
    }

    private void Theme_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is not ComboBox cbox) return;
        switch (cbox.SelectedIndex)
        {
            case 0:
                theme.PreferSystemTheme = true;
                theme.InvalidateThemingFromSystemThemeChanged();
                break;
            case 1:
                theme.PreferSystemTheme = false;
                theme.RequestedTheme = FluentAvaloniaTheme.LightModeString;
                break;
            case 2:
                theme.PreferSystemTheme = false;
                theme.RequestedTheme = FluentAvaloniaTheme.DarkModeString;
                break;
            case 3:
                theme.PreferSystemTheme = false;
                theme.RequestedTheme = FluentAvaloniaTheme.HighContrastModeString;
                break;
        }
    }
}

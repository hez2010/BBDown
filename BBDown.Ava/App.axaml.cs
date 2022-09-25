using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using BBDown.Ava.Services;

namespace BBDown.Ava;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaLocator.CurrentMutable.Bind<ProcessingIndicator>().ToSingleton<ProcessingIndicator>();
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
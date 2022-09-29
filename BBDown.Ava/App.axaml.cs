using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using BBDown.Ava.Services;
using BBDown.Ava.ViewModels;
using System.Net;

namespace BBDown.Ava;

public partial class App : Application
{
    public static readonly HttpClient AppHttpClient;

    static App()
    {
        AppHttpClient = new(new HttpClientHandler
        {
            AllowAutoRedirect = true,
            AutomaticDecompression = DecompressionMethods.All,
            ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
        })
        {
            Timeout = TimeSpan.FromMinutes(5)
        };

        AppHttpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/13.0 Safari/605.1.15");
        AppHttpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
    }

    public override void Initialize()
    {
        AvaloniaLocator.CurrentMutable
            .Bind<MainWindowViewModel>().ToSingleton<MainWindowViewModel>()
            .Bind<UpdateService>().ToSingleton<UpdateService>();
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
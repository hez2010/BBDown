using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Live.Avalonia;
using System.Diagnostics;
using System.Net;

namespace BBDown.Ava;

public partial class App : Application, ILiveView
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
        AvaloniaXamlLoader.Load(this);
    }

    public object CreateView(Window window)
    {
        return new MainView();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            if (Debugger.IsAttached || IsProduction())
            {
                desktop.MainWindow = new MainWindow();
            }
            else
            {
                var window = new LiveViewHost(this, Console.WriteLine);
                desktop.MainWindow = window;
                window.StartWatchingSourceFilesForHotReloading();
            }
        }

        base.OnFrameworkInitializationCompleted();
    }

    public bool IsProduction() =>
#if DEBUG
        false
#else
        true
#endif
    ;
}
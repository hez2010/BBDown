using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Avalonia;
using BBDown.Ava.ViewModels;

namespace BBDown.Ava.Services;

internal class UpdateService
{
    private readonly MainViewModel mainViewModel = AvaloniaLocator.Current.GetRequiredService<MainViewModel>();

    record AssetEntry(
        [property: JsonPropertyName("browser_download_url")]
        string Url,
        [property: JsonPropertyName("size")]
        long Size,
        [property: JsonPropertyName("name")]
        string Name
    );

    record ReleaseEntry(
        [property: JsonPropertyName("html_url")]
        string Url,
        [property: JsonPropertyName("tag_name")]
        string Version,
        [property: JsonPropertyName("name")]
        string Title,
        [property: JsonPropertyName("body")]
        string Content,
        [property: JsonPropertyName("prerelease")]
        bool Prerelease,
        [property: JsonPropertyName("published_at")]
        DateTime PublishedAt,
        [property: JsonPropertyName("assets")]
        AssetEntry[] Assets
    );

    public async Task CheckUpdateAsync()
    {
        mainViewModel.ShowProgressRing = true;
        try
        {
            var ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version!;
            string nowVer = $"{ver.Major}.{ver.Minor}.{ver.Build}";
            var latestRelease = await App.AppHttpClient.GetFromJsonAsync<ReleaseEntry>("https://api.github.com/repos/nilaoda/BBDown/releases/latest");
            if (latestRelease is not null && nowVer != latestRelease.Version)
            {
                mainViewModel.DialogPrimaryButtonText = "Update";
                mainViewModel.DialogCloseButtonText = "Cancel";
                mainViewModel.DialogTitle = $"New version available: {latestRelease.Title}";
                mainViewModel.DialogContent = $"""
                    {latestRelease.Content}

                    {latestRelease.Version} - {latestRelease.PublishedAt:G}
                    """;
                var result = await mainViewModel.ShowDialogAsync();
                if (result == FluentAvalonia.UI.Controls.ContentDialogResult.Primary)
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = latestRelease.Url,
                        UseShellExecute = true
                    })?.Dispose();
                }
            }
        }
        catch { }
        finally
        {
            mainViewModel.ShowProgressRing = false;
        }
    }
}

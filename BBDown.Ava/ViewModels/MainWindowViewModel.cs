using FluentAvalonia.UI.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BBDown.Ava.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public event Func<MainViewModel, Task<ContentDialogResult>>? DialogInvoked;

    private bool showProgressRing;
    public bool ShowProgressRing
    {
        get => showProgressRing;
        set
        {
            showProgressRing = value;
            OnPropertyChanged();
        }
    }

    public async Task<ContentDialogResult?> ShowDialogAsync()
    {
        return DialogInvoked is null ? default : await DialogInvoked.Invoke(this);
    }

    private string? dialogContent;
    public string? DialogContent
    {
        get => dialogContent;
        set
        {
            dialogContent = value;
            OnPropertyChanged();
        }
    }

    private string? dialogTitle;
    public string? DialogTitle
    {
        get => dialogTitle;
        set
        {
            dialogTitle = value;
            OnPropertyChanged();
        }
    }

    private string? dialogPrimaryButtonText;
    public string? DialogPrimaryButtonText
    {
        get => dialogPrimaryButtonText;
        set
        {
            dialogPrimaryButtonText = value;
            OnPropertyChanged();
        }
    }

    private string? dialogSecondaryButtonText;

    public string? DialogSecondaryButtonText
    {
        get => dialogSecondaryButtonText;
        set
        {
            dialogSecondaryButtonText = value;
            OnPropertyChanged();
        }
    }

    private string? dialogCloseButtonText;

    public string? DialogCloseButtonText
    {
        get => dialogCloseButtonText;
        set
        {
            dialogCloseButtonText = value;
            OnPropertyChanged();
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new(propertyName));
    }
}

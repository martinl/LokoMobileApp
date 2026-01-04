namespace loko.Framework;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

public abstract partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    private string title = string.Empty;

    public virtual Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public virtual Task UnInitializeAsync()
    {
        return Task.CompletedTask;
    }


    [RelayCommand]
    private async Task GoBack()
    {
        await GoBackTask();
    }
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {

    }

    public virtual Task GoBackTask()
    {
        return Task.CompletedTask;
    }
}

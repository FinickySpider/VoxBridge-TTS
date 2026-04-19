using System.Windows;
using TtsCommunicationTool.Core.Interfaces;
using TtsCommunicationTool.UI.Views;

namespace TtsCommunicationTool.UI.Services;

public sealed class WpfNotificationService : INotificationService
{
    public void ShowInfo(string message) => ShowToast(message, ToastWindow.ToastLevel.Info);
    public void ShowWarning(string message) => ShowToast(message, ToastWindow.ToastLevel.Warning);
    public void ShowError(string message) => ShowToast(message, ToastWindow.ToastLevel.Error);

    private static void ShowToast(string message, ToastWindow.ToastLevel level)
    {
        if (System.Windows.Application.Current?.Dispatcher is { } dispatcher)
        {
            dispatcher.BeginInvoke(() =>
            {
                var toast = new ToastWindow(message, level);
                toast.Show();
            });
        }
    }
}

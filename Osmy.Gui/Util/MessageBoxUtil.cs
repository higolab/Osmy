using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Threading.Tasks;

namespace Osmy.Gui.Util
{
    internal static class MessageBoxUtil
    {
        /// <summary>
        /// Show information message box (dialog)
        /// </summary>
        /// <param name="text"></param>
        /// <param name="title"></param>
        /// <param name="button"></param>
        /// <param name="icon"></param>
        /// <param name="location"></param>
        /// <param name="owner"></param>
        /// <returns>selected button</returns>
        public static async Task<ButtonResult> ShowInformationDialogAsync(string text, string title = "Information",
            ButtonEnum button = ButtonEnum.Ok, Icon icon = Icon.Info,
            WindowStartupLocation location = WindowStartupLocation.CenterOwner, Window? owner = null)
        {
            return await ShowDialogInternalAsync(text, title, button, icon, location, owner);
        }

        /// <summary>
        /// Show warning message box (dialog)
        /// </summary>
        /// <param name="text"></param>
        /// <param name="title"></param>
        /// <param name="button"></param>
        /// <param name="icon"></param>
        /// <param name="location"></param>
        /// <param name="owner"></param>
        /// <returns>selected button</returns>
        public static async Task<ButtonResult> ShowWarningDialogAsync(string text, string title = "Warning",
            ButtonEnum button = ButtonEnum.Ok, Icon icon = Icon.Warning,
            WindowStartupLocation location = WindowStartupLocation.CenterOwner, Window? owner = null)
        {
            return await ShowDialogInternalAsync(text, title, button, icon, location, owner);
        }

        /// <summary>
        /// Show error message box (dialog)
        /// </summary>
        /// <param name="text"></param>
        /// <param name="title"></param>
        /// <param name="button"></param>
        /// <param name="icon"></param>
        /// <param name="location"></param>
        /// <param name="owner"></param>
        /// <returns>selected button</returns>
        public static async Task<ButtonResult> ShowErrorDialogAsync(string text, string title = "Error",
            ButtonEnum button = ButtonEnum.Ok, Icon icon = Icon.Error,
            WindowStartupLocation location = WindowStartupLocation.CenterOwner, Window? owner = null)
        {
            return await ShowDialogInternalAsync(text, title, button, icon, location, owner);
        }

        /// <summary>
        /// Show message box (dialog)
        /// </summary>
        /// <param name="text"></param>
        /// <param name="title"></param>
        /// <param name="button"></param>
        /// <param name="icon"></param>
        /// <param name="location"></param>
        /// <param name="owner"></param>
        /// <returns>selected button</returns>
        private static async Task<ButtonResult> ShowDialogInternalAsync(string text, string title,
            ButtonEnum button, Icon icon, WindowStartupLocation location, Window? owner)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(title, text, button, icon, location);
            return await box.ShowWindowDialogAsync(owner ?? GetMainWindow());
        }

        /// <summary>
        /// Get main window of this application
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private static Window GetMainWindow()
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                return desktop.MainWindow ?? throw new InvalidOperationException();
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }
}

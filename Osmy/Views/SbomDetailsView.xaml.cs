using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Osmy.Views
{
    /// <summary>
    /// SoftwareDetailsView.xaml の相互作用ロジック
    /// </summary>
    public partial class SbomDetailsView : UserControl
    {
        public SbomDetailsView()
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            var url = e.Uri.ToString();
            try
            {
                Process.Start(url);
            }
            catch
            {
                // ref. https://brockallen.com/2016/09/24/process-start-for-urls-on-net-core/
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }
    }

    public class UrlToHostConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string? sVal = value is string s ? s : value.ToString();

            if (Uri.TryCreate(sVal, UriKind.Absolute, out var uri))
            {
                return uri.Host;
            }
            else
            {
                return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

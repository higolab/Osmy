using Avalonia.Controls;
using Avalonia.Data.Converters;
using OSV.Client.Models;
using OSV.Schema;
using System;
using System.Globalization;

namespace Osmy.Gui.Controls
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

        //private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        //{
        //    var url = e.Uri.ToString();
        //    try
        //    {
        //        Process.Start(url);
        //    }
        //    catch
        //    {
        //        // ref. https://brockallen.com/2016/09/24/process-start-for-urls-on-net-core/
        //        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        //        {
        //            url = url.Replace("&", "^&");
        //            Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
        //        }
        //        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        //        {
        //            Process.Start("xdg-open", url);
        //        }
        //        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        //        {
        //            Process.Start("open", url);
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }
        //}
    }

    public class UrlToHostConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            string? sVal = value is string s ? s : value?.ToString();

            if (Uri.TryCreate(sVal, UriKind.Absolute, out var uri))
            {
                return uri.Host;
            }
            else
            {
                return sVal ?? string.Empty;
            }
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class EqualsConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return Equals(value, parameter);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NotEqualsConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return !Equals(value, parameter);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public static class CompareConverters
    {
        public static readonly EqualsConverter EqualsConverter = new();

        public static readonly NotEqualsConverter NotEqualsConverter = new();
    }
}

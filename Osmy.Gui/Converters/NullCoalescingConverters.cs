using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Osmy.Gui.Converters
{
    public static class NullCoalescingConverters
    {
        public static readonly IValueConverter Normal = new NullCoalescingConverter();

        public static readonly IValueConverter DefaultFalse = new DefaultFalseNullCoalescingConverter();
    }

    public class NullCoalescingConverter : IValueConverter
    {
        public virtual object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value ?? parameter;
        }

        public virtual object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DefaultFalseNullCoalescingConverter : NullCoalescingConverter
    {
        public override object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return base.Convert(value, targetType, parameter ?? false, culture);
        }
    }
}

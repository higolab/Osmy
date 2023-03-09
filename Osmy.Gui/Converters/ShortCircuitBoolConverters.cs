using Avalonia.Data.Converters;
using Avalonia;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Osmy.Gui.Converters
{
    public static class ShortCircuitBoolConverters
    {
        public static readonly IMultiValueConverter And = new ShortCircuitAndConverter();

        public static readonly IMultiValueConverter Or = new ShortCircuitOrConverter();
    }

    public class ShortCircuitAndConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            foreach (object? value in values)
            {
                if (value is bool bValue && !bValue)
                {
                    return false;
                }
                else if (value == AvaloniaProperty.UnsetValue)
                {
                    return false;
                }
            }

            return true;
        }
    }

    public class ShortCircuitOrConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            foreach (object? value in values)
            {
                if (value is bool bValue && bValue)
                {
                    return true;
                }
            }

            return false;
        }
    }
}

using Avalonia;
using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Osmy.Gui.Converters
{
    public static class CollectionConverters
    {
        public static readonly IValueConverter Empty = new CollectionEmptyConverter();

        public static readonly IValueConverter NotEmpty = new CollectionNotEmptyConverter();
    }

    public class CollectionEmptyConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is IEnumerable<object> enumerable)
            {
                return !enumerable.Any();
            }
            else
            {
                return AvaloniaProperty.UnsetValue;
            }
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CollectionNotEmptyConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is IEnumerable<object> enumerable)
            {
                return enumerable.Any();
            }
            else
            {
                return AvaloniaProperty.UnsetValue;
            }
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

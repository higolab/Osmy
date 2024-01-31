using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace Osmy.Gui.Converters
{
    public class ItemIndexConverter : MarkupExtension, IMultiValueConverter
    {
        public static readonly IMultiValueConverter OneBased = new ItemIndexConverter() { Base = 1 };

        public int Base { get; init; }

        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values is [IEnumerable list, object or null])
            {
                object? value = values[1];
                int index = Base;
                foreach (var item in list)
                {
                    if (item == value)
                    {
                        return index.ToString();
                    }
                    index++;
                }
            }

            return AvaloniaProperty.UnsetValue;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return OneBased;
        }
    }
}

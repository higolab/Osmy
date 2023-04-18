using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Osmy.Gui.Converters
{
    public static class CompareConverters
    {
        public static readonly EqualsConverter EqualsConverter = new();

        public static readonly NotEqualsConverter NotEqualsConverter = new();
    }

    public class EqualsConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return ValueComparer.Compare(value, parameter);
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
            return !ValueComparer.Compare(value, parameter);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    internal static class ValueComparer
    {
        /// <summary>
        /// 2つの値が等しいか判定します．
        /// </summary>
        /// <param name="objA"></param>
        /// <param name="objB"></param>
        /// <returns><paramref name="objA"/>と<paramref name="objB"/>が等しければtrue，それ以外はfalse</returns>
        /// <remarks>
        /// <paramref name="objB"/>が<see langword="string"/>かつ<paramref name="objA"/>が主要なプリミティブ型の場合は，
        /// <paramref name="objB"/>を<paramref name="objA"/>の型に変換して比較を行います．
        /// </remarks>
        public static bool Compare(object? objA, object? objB)
        {
            // objBが文字列の場合は，可能であればobjAの型に変換して比較する
            if (objB is string sparam)
            {
                return objA switch
                {
                    byte byteValue => byte.TryParse(sparam, out byte byteParam) && byteValue == byteParam,
                    char charValue => char.TryParse(sparam, out char charParam) && charValue == charParam,
                    short shortValue => short.TryParse(sparam, out short shortParam) && shortValue == shortParam,
                    ushort ushortValue => ushort.TryParse(sparam, out ushort ushortParam) && ushortValue == ushortParam,
                    int intValue => int.TryParse(sparam, out int intParam) && intValue == intParam,
                    uint uintValue => uint.TryParse(sparam, out uint uintParam) && uintValue == uintParam,
                    long longValue => long.TryParse(sparam, out long longParam) && longValue == longParam,
                    ulong ulongValue => ulong.TryParse(sparam, out ulong ulongParam) && ulongValue == ulongParam,
                    string stringValue => stringValue == sparam,
                    bool boolValue => bool.TryParse(sparam, out bool boolParam) && boolValue == boolParam,
                    _ => false,
                };
            }

            return Equals(objA, objB);
        }
    }
}

using System;
using System.Globalization;
using Avalonia.Media;
using Avalonia.Data.Converters;

namespace AutoSettingsPage.Avalonia;

public sealed class DoubleDecimalConverter : IValueConverter
{
    public static readonly DoubleDecimalConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is double d ? (decimal) d : null;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is decimal m ? (double) m : 0d;
}

public sealed class IntDecimalConverter : IValueConverter
{
    public static readonly IntDecimalConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is int number ? (decimal) number : null;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is decimal number ? (int) number : 0;
}

public static class ColorValueConverter
{
    public static FuncValueConverter<uint, Color> Instance { get; } = new(
        static value => Color.FromUInt32(value),
        static color => color.ToUInt32());
}

public static class NullableValueConverters
{
    public static FuncValueConverter<bool, bool?> Bool { get; } = new(
        static value => value,
        static value => value is true);

    public static FuncValueConverter<DateTime, DateTime?> DateTime { get; } = new(
        static value => value,
        static value => value.GetValueOrDefault());
}

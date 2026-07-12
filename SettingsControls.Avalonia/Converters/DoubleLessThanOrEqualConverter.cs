using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace CommunityToolkit.Avalonia.Controls.Converters;

public sealed class DoubleLessThanOrEqualConverter : IValueConverter
{
    public double Threshold { get; set; }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is double number && number <= Threshold;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

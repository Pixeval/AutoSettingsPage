using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace CommunityToolkit.Avalonia.Controls.Converters;

internal enum CornerRadiusFilter
{
    Top,
    Right,
    Bottom,
    Left
}

/// <summary>
/// Preserves only one side of a <see cref="CornerRadius"/>.
/// </summary>
internal class CornerRadiusFilterConverter : IValueConverter
{
    public CornerRadiusFilter Filter { get; set; }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not CornerRadius radius)
            return value;

        return Filter switch
        {
            CornerRadiusFilter.Top => new CornerRadius(radius.TopLeft, radius.TopRight, 0, 0),
            CornerRadiusFilter.Right => new CornerRadius(0, radius.TopRight, radius.BottomRight, 0),
            CornerRadiusFilter.Bottom => new CornerRadius(0, 0, radius.BottomRight, radius.BottomLeft),
            CornerRadiusFilter.Left => new CornerRadius(radius.TopLeft, 0, 0, radius.BottomLeft),
            _ => throw new ArgumentOutOfRangeException(nameof(Filter), Filter, null)
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => value;
}

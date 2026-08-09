using System.Linq.Expressions;
using System.Numerics;
using FluentIcons.Common;

namespace AutoSettingsPage.Models;

public class NumberSettingsEntry<TSettings, TNumber> : SingleValueSettingsEntry<TSettings, TNumber>, INumberSettingsEntry<TNumber>
    where TNumber : INumber<TNumber>, IMinMaxValue<TNumber>
{
    public NumberSettingsEntry(
        TSettings settings,
        string token,
        string header,
        string description,
        Symbol icon,
        string? placeholder,
        Func<TSettings, TNumber> getter,
        Action<TSettings, TNumber> setter)
        : base(settings, token, header, description, icon, placeholder, getter, setter)
    {
    }

    public NumberSettingsEntry(
        TSettings settings,
        string token,
        SettingsEntryAttribute attribute,
        Func<TSettings, TNumber> getter,
        Action<TSettings, TNumber> setter)
        : base(settings, token, attribute, getter, setter)
    {
    }

    public NumberSettingsEntry(
        TSettings settings,
        Expression<Func<TSettings, TNumber>> property)
        : base(settings, property)
    {
    }

    /// <inheritdoc />
    public TNumber Max { get; set; } = TNumber.MaxValue;

    /// <inheritdoc />
    public TNumber Min { get; set; } = TNumber.MinValue;

    /// <inheritdoc />
    public TNumber Step { get; set; } = TNumber.One;
}

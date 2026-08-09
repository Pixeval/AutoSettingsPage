using System;
using System.Linq.Expressions;
using AutoSettingsPage.Models;
using FluentIcons.Common;

namespace AutoSettingsPage.WinUI;

public partial class ColorSettingsEntry<TSettings> : NumberSettingsEntry<TSettings, uint>
{
    public ColorSettingsEntry(
        TSettings settings,
        string token,
        string header,
        string description,
        Symbol icon,
        string? placeholder,
        Func<TSettings, uint> getter,
        Action<TSettings, uint> setter)
        : base(settings, token, header, description, icon, placeholder, getter, setter)
    {
    }

    public ColorSettingsEntry(
        TSettings settings,
        string token,
        SettingsEntryAttribute attribute,
        Func<TSettings, uint> getter,
        Action<TSettings, uint> setter)
        : base(settings, token, attribute, getter, setter)
    {
    }

    public ColorSettingsEntry(
        TSettings settings,
        Expression<Func<TSettings, uint>> property)
        : base(settings, property)
    {
    }
}

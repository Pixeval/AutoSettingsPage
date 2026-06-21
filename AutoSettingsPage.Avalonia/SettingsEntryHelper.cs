using System;
using System.Linq.Expressions;
using AutoSettingsPage.Models;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Fonts;

namespace AutoSettingsPage.Avalonia;

public static class SettingsEntryHelper
{
    public static IFontCollection AvailableFonts => FontManager.Current.SystemFonts;

    public static FuncValueConverter<ISettingsEntry, object?> DescriptionConverter { get; } = new(DescriptionControl);

    public static object? DescriptionControl(ISettingsEntry? entry)
    {
        if (entry?.DescriptionUri is not null)
        {
            var b = new HyperlinkButton
            {
                Padding = new(0),
                Content = entry.Description,
                NavigateUri = entry.DescriptionUri
            };
            return b;
        }

        return entry?.Description;
    }

    public static Control GetControl(ISettingsEntry entry) => FactoryDictionary[entry.GetType()](entry);

    public static SettingsEntryControlFactoryDictionary<Control> FactoryDictionary { get; } = new();

    extension(SettingsEntryControlFactoryDictionary<Control> dictionary)
    {
        public SettingsEntryControlFactoryDictionary<Control> Add<TEntry, TControl>()
            where TEntry : ISettingsEntry
            where TControl : Control, IEntryControl<TEntry>, new() =>
            dictionary.Add<TEntry>(static entry => new TControl { Entry = entry });

        public SettingsEntryControlFactoryDictionary<Control> AddAssignable<TEntry, TControl>()
            where TEntry : ISettingsEntry
            where TControl : Control, IEntryControl<TEntry>, new() =>
            dictionary.AddAssignable<TEntry>(static entry => new TControl { Entry = entry });

        public SettingsEntryControlFactoryDictionary<Control> AddOpenGeneric<TEntry, TControl>(
            Type openGenericEntryType)
            where TEntry : ISettingsEntry
            where TControl : Control, IEntryControl<TEntry>, new() =>
            dictionary.AddOpenGeneric(openGenericEntryType, static entry => new TControl { Entry = (TEntry)entry });

        public SettingsEntryControlFactoryDictionary<Control> AddPredefined() =>
            dictionary
                .Add<ClickableSettingsEntry, ClickableSettingsCard>()
                .AddAssignable<IMultiValuesWithSwitchSettingsEntry, MultiValuesWithSwitchSettingsExpander>()
                .AddAssignable<IMultiValuesSettingsEntry, MultiValuesSettingsExpander>()
                .AddAssignable<IEnumSettingsEntry<object>, EnumSettingsCard>()
                .AddAssignable<INumberSettingsEntry<double>, DoubleSettingsCard>()
                .AddAssignable<INumberSettingsEntry<int>, DoubleSettingsCard>()
                .AddAssignable<ISingleValueSettingsEntry<string>, StringSettingsCard>()
                .AddAssignable<ISingleValueSettingsEntry<bool>, BoolSettingsCard>()
                .AddAssignable<IColorSettingsEntry, ColorSettingsCard>()
                .AddAssignable<ISingleValueSettingsEntry<DateTime>, DateSettingsCard>();
    }

    extension<TSettings>(ISettingsGroupBuilder<TSettings> builder)
    {
        public ISettingsGroupBuilder<TSettings> Color(
            Expression<Func<TSettings, uint>> property,
            Action<ColorSettingsEntry<TSettings>>? config = null) =>
            builder.Add(new(builder.Settings, property), config);
    }
}

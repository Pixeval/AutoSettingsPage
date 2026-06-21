using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using AutoSettingsPage.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.System;

namespace AutoSettingsPage.WinUI;

public static class SettingsEntryHelper
{
    public static IReadOnlyList<string> AvailableFonts { get; set; } = [];

    public static object DescriptionControl(this ISettingsEntry entry)
    {
        if (entry.DescriptionUri is not null)
        {
            var b = new HyperlinkButton { Content = entry.Description };
            if (entry.DescriptionUri.Scheme is "http" or "https")
            {
                b.NavigateUri = entry.DescriptionUri;
                return b;
            }

            var uri = entry.DescriptionUri;
            b.Click += (_, _) => _ = Launcher.LaunchUriAsync(uri);
            return b;
        }

        return entry.Description;
    }

    public static FrameworkElement GetControl(ISettingsEntry entry) => FactoryDictionary[entry.GetType()](entry);

    public static SettingsEntryControlFactoryDictionary<FrameworkElement> FactoryDictionary { get; } = new();

    extension(SettingsEntryControlFactoryDictionary<FrameworkElement> dictionary)
    {
        public SettingsEntryControlFactoryDictionary<FrameworkElement> Add<TEntry, TControl>()
            where TEntry : ISettingsEntry
            where TControl : FrameworkElement, IEntryControl<TEntry>, new() =>
            dictionary.Add<TEntry>(static entry => new TControl { Entry = entry });

        public SettingsEntryControlFactoryDictionary<FrameworkElement> AddAssignable<TEntry, TControl>()
            where TEntry : ISettingsEntry
            where TControl : FrameworkElement, IEntryControl<TEntry>, new() =>
            dictionary.AddAssignable<TEntry>(static entry => new TControl { Entry = entry });

        public SettingsEntryControlFactoryDictionary<FrameworkElement> AddOpenGeneric<TEntry, TControl>(
            Type openGenericEntryType)
            where TEntry : ISettingsEntry
            where TControl : FrameworkElement, IEntryControl<TEntry>, new() =>
            dictionary.AddOpenGeneric(openGenericEntryType, static entry => new TControl { Entry = (TEntry) entry });


        public SettingsEntryControlFactoryDictionary<FrameworkElement> AddPredefined() =>
            dictionary
                .Add<ClickableSettingsEntry, ClickableSettingsCard>()
                .AddOpenGeneric<ISingleValueSettingsEntry<string>, FontSettingsCard>(typeof(FontSettingsEntry<>))
                .AddOpenGeneric<ISingleValueSettingsEntry<ObservableCollection<string>>, TokenizingSettingsExpander>(typeof(CollectionSettingsEntry<,>))
                .AddOpenGeneric<ISingleValueSettingsEntry<uint>, ColorSettingsCard>(typeof(ColorSettingsEntry<>))
                .AddAssignable<IMultiValuesWithSwitchSettingsEntry, MultiValuesWithSwitchSettingsExpander>()
                .AddAssignable<IMultiValuesSettingsEntry, MultiValuesSettingsExpander>()
                .AddAssignable<IEnumSettingsEntry<object>, EnumSettingsCard>()
                .AddAssignable<INumberSettingsEntry<double>, DoubleSettingsCard>()
                .AddAssignable<INumberSettingsEntry<int>, DoubleSettingsCard>()
                .AddAssignable<ISingleValueSettingsEntry<string>, StringSettingsCard>()
                .AddAssignable<ISingleValueSettingsEntry<bool>, BoolSettingsCard>()
                .AddAssignable<ISingleValueSettingsEntry<DateTimeOffset>, DateSettingsCard>();
    }

    extension<TSettings>(ISettingsGroupBuilder<TSettings> builder)
    {
        public ISettingsGroupBuilder<TSettings> Color(
            Expression<Func<TSettings, uint>> property,
            Action<ColorSettingsEntry<TSettings>>? config = null) =>
            builder.Add(new(builder.Settings, property), config);

        public ISettingsGroupBuilder<TSettings> Font(
            Expression<Func<TSettings, string>> property,
            Action<FontSettingsEntry<TSettings>>? config = null) =>
            builder.Add(new(builder.Settings, property), config);
    }
}

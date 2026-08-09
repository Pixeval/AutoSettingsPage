using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Numerics;
using AutoSettingsPage.Models;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Fonts;
using CommunityToolkit.Avalonia.Controls;
using FluentIcons.Avalonia;

namespace AutoSettingsPage.Avalonia;

public static class SettingsEntryHelper
{
    public readonly record struct ValueControlMapping(
        IDataTemplate ValueTemplate,
        bool IsExpander,
        bool IsEnabledWithMainValue);

    public static IFontCollection AvailableFonts => FontManager.Current.SystemFonts;

    public static FuncValueConverter<ISettingsEntry, object?> DescriptionConverter { get; } = new(DescriptionControl);

    public static FuncValueConverter<IReadOnlySingleValueSettingsEntry, IDataTemplate?> ValueTemplateConverter { get; } = new(static entry => entry is null ? null : ResolveValueControlMapping(entry).ValueTemplate);

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

    public static Control GetControl(ISettingsEntry entry)
    {
        if (FactoryDictionary.TryGetFactory(entry.GetType(), out var factory))
            return factory(entry);
        if (entry is IMultiValuesWithMainValueSettingsEntry multiValues)
            return new MultiValuesWithMainValueSettingsExpander { Entry = multiValues };
        if (entry is IMultiValuesSettingsEntry multiValuesEntry)
            return new MultiValuesSettingsExpander { Entry = multiValuesEntry };
        if (entry is IReadOnlySingleValueSettingsEntry singleValueEntry)
            return GetSingleValueControl(entry, ResolveValueControlMapping(singleValueEntry));
        return FactoryDictionary[entry.GetType()](entry);
    }

    public static Control GetValueControl(
        IReadOnlySingleValueSettingsEntry entry,
        IMultiValuesWithMainValueSettingsEntry? parent = null)
    {
        var control = new ContentPresenter
        {
            Content = entry,
            ContentTemplate = ResolveValueControlMapping(entry).ValueTemplate
        };
        if (parent is not null)
            ApplyMainValueEnabled(control, parent, ResolveValueControlMapping(parent.MainValue));
        return control;
    }

    internal static ValueControlMapping ResolveValueControlMapping(IReadOnlySingleValueSettingsEntry entry)
    {
        if (ValueFactoryDictionary.TryGetFactory(entry.GetType(), out var factory))
            return factory(entry);
        throw new KeyNotFoundException($"No settings value control factory is registered for '{entry.GetType()}'.");
    }

    private static Control GetSingleValueControl(ISettingsEntry entry, ValueControlMapping mapping)
    {
        var icon = new SymbolIcon { Symbol = entry.Icon };
        if (mapping.IsExpander)
        {
            return new SettingsExpander
            {
                DataContext = entry,
                Header = entry.Header,
                Description = DescriptionControl(entry),
                HeaderIcon = icon,
                Tag = entry.Token,
                Content = entry,
                ContentTemplate = mapping.ValueTemplate
            };
        }

        return new SettingsCard
        {
            DataContext = entry,
            Header = entry.Header,
            Description = DescriptionControl(entry),
            HeaderIcon = icon,
            Tag = entry.Token,
            Content = entry,
            ContentTemplate = mapping.ValueTemplate
        };
    }

    internal static Control GetChildControl(
        ISettingsEntry entry,
        IMultiValuesWithMainValueSettingsEntry parent,
        ValueControlMapping mapping)
    {
        var control = GetControl(entry);
        ApplyMainValueEnabled(control, parent, mapping);
        return control;
    }

    private static void ApplyMainValueEnabled(
        Control control,
        IMultiValuesWithMainValueSettingsEntry parent,
        ValueControlMapping mapping)
    {
        if (mapping.IsEnabledWithMainValue && parent.MainValue is ISingleValueSettingsEntry<bool> boolMainValue)
            control[!InputElement.IsEnabledProperty] =
                CompiledBinding.Create<ISingleValueSettingsEntry<bool>, bool>(
                    value => value.Value,
                    boolMainValue);
    }

    public static SettingsEntryControlFactoryDictionary<Control> FactoryDictionary { get; } = new();

    public static SettingsEntryControlFactoryDictionary<ValueControlMapping> ValueFactoryDictionary { get; } = new();

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
            dictionary.AddOpenGeneric(openGenericEntryType, static entry => new TControl { Entry = (TEntry) entry });

        public SettingsEntryControlFactoryDictionary<Control> AddPredefined()
        {
            dictionary
                .Add<ClickableSettingsEntry, ClickableSettingsCard>();

            ValueFactoryDictionary
                .AddValue<IEnumSettingsEntry<object>>(DataTemplates.EnumValueTemplate)
                .AddValue<INumberSettingsEntry<double>>(DataTemplates.DoubleValueTemplate)
                .AddValue<INumberSettingsEntry<int>>(DataTemplates.IntValueTemplate)
                .AddValue<ISingleValueSettingsEntry<string>>(DataTemplates.StringValueTemplate)
                .AddValue<ISingleValueSettingsEntry<bool>>(DataTemplates.BoolValueTemplate,
                    isEnabledWithMainValue: static _ => true)
                .AddValue<IColorSettingsEntry>(DataTemplates.ColorValueTemplate)
                .AddValue<ISingleValueSettingsEntry<DateTime>>(DataTemplates.DateValueTemplate);
            return dictionary;
        }
    }

    extension(SettingsEntryControlFactoryDictionary<ValueControlMapping> dictionary)
    {
        public SettingsEntryControlFactoryDictionary<ValueControlMapping> AddValue<TEntry>(
            IDataTemplate template,
            Func<TEntry, bool>? isExpander = null,
            Func<TEntry, bool>? isEnabledWithMainValue = null)
            where TEntry : IReadOnlySingleValueSettingsEntry =>
            dictionary.Add<TEntry>(entry => new(
                template,
                isExpander?.Invoke(entry) ?? entry is IMultiValuesSettingsEntry,
                isEnabledWithMainValue?.Invoke(entry) is true));

        public SettingsEntryControlFactoryDictionary<ValueControlMapping> AddOpenGenericValue<TEntry>(
            Type openGenericEntryType,
            IDataTemplate template,
            Func<TEntry, bool>? isExpander = null,
            Func<TEntry, bool>? isEnabledWithMainValue = null)
            where TEntry : IReadOnlySingleValueSettingsEntry =>
            dictionary.AddOpenGeneric(openGenericEntryType, entry =>
            {
                var valueEntry = (TEntry) entry;
                return new(
                    template,
                    isExpander?.Invoke(valueEntry) ?? valueEntry is IMultiValuesSettingsEntry,
                    isEnabledWithMainValue?.Invoke(valueEntry) is true);
            });
    }

    extension<TSettings>(ISettingsGroupBuilder<TSettings> builder)
    {
        public ISettingsGroupBuilder<TSettings> Color(
            Expression<Func<TSettings, uint>> property,
            Action<ColorSettingsEntry<TSettings>>? config = null) =>
            builder.Add(new(builder.Settings, property), config);
    }
}

file static class DataTemplates
{
    internal static IDataTemplate BoolValueTemplate { get; } = new FuncDataTemplate<ISingleValueSettingsEntry<bool>>(static (entry, _) => new ToggleSwitch
    {
        [!ToggleButton.IsCheckedProperty] = CompiledBinding.Create<ISingleValueSettingsEntry<bool>, bool>(
            value => value.Value,
            entry,
            converter: NullableValueConverters.Bool,
            mode: BindingMode.TwoWay)
    });

    internal static IDataTemplate StringValueTemplate { get; } = new FuncDataTemplate<ISingleValueSettingsEntry<string>>(static (entry, _) => new TextBox
    {
        Width = 200,
        PlaceholderText = entry.Placeholder,
        [!TextBox.TextProperty] = CompiledBinding.Create<ISingleValueSettingsEntry<string>, string?>(
            value => value.Value,
            entry,
            mode: BindingMode.TwoWay)
    });

    internal static IDataTemplate DateValueTemplate { get; } = new FuncDataTemplate<ISingleValueSettingsEntry<DateTime>>(static (entry, _) => new CalendarDatePicker
    {
        PlaceholderText = entry.Placeholder,
        [!CalendarDatePicker.SelectedDateProperty] =
            CompiledBinding.Create<ISingleValueSettingsEntry<DateTime>, DateTime>(
                value => value.Value,
                entry,
                converter: NullableValueConverters.DateTime,
                mode: BindingMode.TwoWay)
    });

    internal static IDataTemplate DoubleValueTemplate { get; } = new FuncDataTemplate<INumberSettingsEntry<double>>(static (entry, _) => CreateNumberValue(
        entry,
        DoubleDecimalConverter.Instance));

    internal static IDataTemplate IntValueTemplate { get; } = new FuncDataTemplate<INumberSettingsEntry<int>>(static (entry, _) => CreateNumberValue(
        entry,
        IntDecimalConverter.Instance));

    internal static IDataTemplate EnumValueTemplate { get; } = new FuncDataTemplate<IEnumSettingsEntry<object>>(static (entry, _) => new EnumComboBox
    {
        Width = 200,
        ItemsSource = entry.EnumItems,
        PlaceholderText = entry.Placeholder,
        [!EnumComboBox.SelectedEnumProperty] = CompiledBinding.Create<IEnumSettingsEntry<object>, object?>(
            value => value.Value,
            entry,
            mode: BindingMode.TwoWay)
    });

    internal static IDataTemplate ColorValueTemplate { get; } = new FuncDataTemplate<IColorSettingsEntry>(static (entry, _) => new ColorPicker
    {
        [!ColorView.ColorProperty] = CompiledBinding.Create<IColorSettingsEntry, uint>(
            value => value.Value,
            entry,
            converter: ColorValueConverter.Instance,
            mode: BindingMode.TwoWay)
    });

    internal static NumericUpDown CreateNumberValue<TNumber>(INumberSettingsEntry<TNumber> entry, IValueConverter converter)
        where TNumber : INumberBase<TNumber>
    {
        var control = new NumericUpDown
        {
            MinWidth = 200,
            FormatString = "F0",
            PlaceholderText = entry.Placeholder,
            [!NumericUpDown.IncrementProperty] = CompiledBinding.Create<INumberSettingsEntry<TNumber>, TNumber>(
                value => value.Step,
                entry,
                converter: converter),
            [!NumericUpDown.MaximumProperty] = CompiledBinding.Create<INumberSettingsEntry<TNumber>, TNumber>(
                value => value.Max,
                entry,
                converter: converter),
            [!NumericUpDown.MinimumProperty] = CompiledBinding.Create<INumberSettingsEntry<TNumber>, TNumber>(
                value => value.Min,
                entry,
                converter: converter),
            [!NumericUpDown.ValueProperty] = CompiledBinding.Create<INumberSettingsEntry<TNumber>, TNumber>(
                value => value.Value,
                entry,
                converter: converter,
                mode: BindingMode.TwoWay)
        };
        return control;
    }
}

using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Numerics;
using AutoSettingsPage.Models;
using FluentIcons.Common;

namespace AutoSettingsPage;

public static class SettingsBuilder
{
    public static ISettingsGroupBuilder<TSettings> CreateGroup<TSettings>(TSettings settings) =>
        new SettingsGroupBuilderImpl<TSettings>(settings);

    public static ISettingsGroupListBuilder<TSettings> CreateGroupList<TSettings>(TSettings settings) =>
        new SettingsGroupListBuilderImpl<TSettings>(settings);

    extension<TSettings>(ISettingsGroupBuilder<TSettings> builder)
    {
        public ISettingsGroupBuilder<TSettings> Clickable(
            string token,
            string header,
            string description,
            Symbol icon,
            EventHandler<ClickableSettingsEntry, EventArgs> clicked,
            Action<ClickableSettingsEntry>? config = null) =>
            builder.Add(new(token, header, description, icon, clicked), config);

        public ISettingsGroupBuilder<TSettings> Clickable(
            string header,
            string description,
            Symbol icon,
            EventHandler<ClickableSettingsEntry, EventArgs> clicked,
            Action<ClickableSettingsEntry>? config = null) =>
            builder.Clickable(header, header, description, icon, clicked, config);

        public ISettingsGroupBuilder<TSettings> SingleValue<TValue>(
            Expression<Func<TSettings, TValue>> property,
            Action<SingleValueSettingsEntry<TSettings, TValue>>? config = null) =>
            builder.Add(new(builder.Settings, property), config);

        public ISettingsGroupBuilder<TSettings> SingleValueWithOption<TValue, TOption>(
            Expression<Func<TSettings, TValue>> property,
            TOption option,
            Action<SingleValueWithOptionSettingsEntry<TSettings, TValue, TOption>>? config = null) =>
            builder.Add(new(builder.Settings, property, option), config);

        public ISettingsGroupBuilder<TSettings> String(
            Expression<Func<TSettings, string>> property,
            Action<StringSettingsEntry<TSettings>>? config = null) =>
            builder.Add(new(builder.Settings, property), config);

        public ISettingsGroupBuilder<TSettings> Number<TNumber>(
            Expression<Func<TSettings, TNumber>> property,
            TNumber min, TNumber max, TNumber step,
            Action<NumberSettingsEntry<TSettings, TNumber>>? config = null)
            where TNumber : INumber<TNumber>, IMinMaxValue<TNumber> =>
            builder.Add(new(builder.Settings, property)
            {
                Min = min,
                Max = max,
                Step = step
            }, config);

        public ISettingsGroupBuilder<TSettings> Int(
            Expression<Func<TSettings, int>> property,
            int min, int max, int step,
            Action<NumberSettingsEntry<TSettings, int>>? config = null) =>
            builder.Number(property, min, max, step, config);

        public ISettingsGroupBuilder<TSettings> Double(
            Expression<Func<TSettings, double>> property,
            double min, double max, double step,
            Action<NumberSettingsEntry<TSettings, double>>? config = null) =>
            builder.Number(property, min, max, step, config);

        public ISettingsGroupBuilder<TSettings> UInt(
            Expression<Func<TSettings, uint>> property,
            uint min, uint max, uint step,
            Action<NumberSettingsEntry<TSettings, uint>>? config = null) =>
            builder.Number(property, min, max, step, config);

        public ISettingsGroupBuilder<TSettings> DateTimeOffset(
            Expression<Func<TSettings, DateTimeOffset>> property,
            DateTimeOffset min, DateTimeOffset max,
            Action<DateTimeOffsetSettingsEntry<TSettings>>? config = null) =>
            builder.Add(new(builder.Settings, property)
            {
                Min = min,
                Max = max
            }, config);

        public ISettingsGroupBuilder<TSettings> DateTime(
            Expression<Func<TSettings, DateTime>> property,
            DateTime min, DateTime max,
            Action<DateTimeSettingsEntry<TSettings>>? config = null) =>
            builder.Add(new(builder.Settings, property)
            {
                Min = min,
                Max = max
            }, config);

        public ISettingsGroupBuilder<TSettings> Bool(
            Expression<Func<TSettings, bool>> property,
            Action<BoolSettingsEntry<TSettings>>? config = null) =>
            builder.Add(new(builder.Settings, property), config);

        public ISettingsGroupBuilder<TSettings> Enum<TEnum>(
            Expression<Func<TSettings, TEnum>> property,
            IReadOnlyList<IReadOnlyStringPair<TEnum>> enumItems,
            Action<EnumSettingsEntry<TSettings, TEnum>>? config = null) =>
            builder.Add(new(builder.Settings, property, enumItems), config);

        public ISettingsGroupBuilder<TSettings> Collection<TItem>(
            Expression<Func<TSettings, ObservableCollection<TItem>>> property,
            Action<CollectionSettingsEntry<TSettings, TItem>>? config = null) =>
            builder.Add(new(builder.Settings, property), config);

        public ISettingsGroupBuilder<TSettings> MultiValues<TSubSettings>(
            Expression<Func<TSettings, TSubSettings>> subSettingsProperty,
            Action<ISettingsGroupBuilder<TSubSettings>>? configValues,
            Action<MultiValuesEntry<TSettings, TSubSettings>>? config = null)
        {
            var subSettings = subSettingsProperty.Compile(preferInterpretation: true)(builder.Settings)
                ?? throw new InvalidOperationException("The sub-settings property returned null.");
            var entries = CreateGroup(subSettings);
            configValues?.Invoke(entries);
            return builder.Add(new(subSettingsProperty, entries.Build()), config);
        }

        public ISettingsGroupBuilder<TSettings> MultiValuesWithSwitch<TSubSettings>(
            Expression<Func<TSettings, TSubSettings>> subSettingsProperty,
            Expression<Func<TSubSettings, bool>> mainValueProperty,
            Action<ISettingsGroupBuilder<TSubSettings>>? configValues,
            Action<MultiValuesWithMainValueEntry<TSettings, TSubSettings, BoolSettingsEntry<TSubSettings>>>? config = null) =>
            builder.MultiValuesWithMainValue(subSettingsProperty, mainValueProperty,
                static (settings, property) => new BoolSettingsEntry<TSubSettings>(settings, property), configValues, config);

        public ISettingsGroupBuilder<TSettings> MultiValuesWithMainValue<TSubSettings, TValue, TEntry>(
            Expression<Func<TSettings, TSubSettings>> subSettingsProperty,
            Expression<Func<TSubSettings, TValue>> mainValueProperty,
            Func<TSubSettings, Expression<Func<TSubSettings, TValue>>, TEntry> mainValueFactory,
            Action<ISettingsGroupBuilder<TSubSettings>>? configValues,
            Action<MultiValuesWithMainValueEntry<TSettings, TSubSettings, TEntry>>? config = null)
            where TEntry : IReadOnlySingleValueSettingsEntry
        {
            // Resolve once so the main value and child entries always edit the same instance.
            var subSettings = subSettingsProperty.Compile(preferInterpretation: true)(builder.Settings)
                ?? throw new InvalidOperationException("The sub-settings property returned null.");
            var entries = CreateGroup(subSettings);
            configValues?.Invoke(entries);
            return builder.Add(new MultiValuesWithMainValueEntry<TSettings, TSubSettings, TEntry>(
                subSettings, subSettingsProperty, mainValueFactory(subSettings, mainValueProperty), entries.Build()), config);
        }
    }

    private class SettingsGroupBuilderImpl<TSettings> : ISettingsGroupBuilder<TSettings>
    {
        public TSettings Settings { get; }

        private readonly List<ISettingsEntry> _entries = [];

        private ISettingsEntry? _currentEntry;

        internal SettingsGroupBuilderImpl(TSettings settings) => Settings = settings;

        /// <inheritdoc />
        public ISettingsGroupBuilder<TSettings> Add<TEntry>(
            TEntry entry,
            Action<TEntry>? config = null)
            where TEntry : ISettingsEntry
        {
            _currentEntry = entry;
            _entries.Add(entry);
            config?.Invoke(entry);
            return this;
        }

        /// <inheritdoc />
        public ISettingsGroupBuilder<TSettings> ConfigLast(Action<ISettingsEntry> config)
        {
            if (_currentEntry is null)
                throw new InvalidOperationException("No entry has been added to configure.");
            config(_currentEntry);
            return this;
        }

        /// <inheritdoc />
        public IReadOnlyList<ISettingsEntry> Build() => _entries;

        public ISettingsGroupBuilder<TSettings> AddRange(IReadOnlyList<ISettingsEntry> entries)
        {
            if (entries is not [.., var last])
                return this;
            _currentEntry = last;
            _entries.AddRange(entries);
            return this;
        }
    }

    private class SettingsGroupListBuilderImpl<TSettings> : ISettingsGroupListBuilder<TSettings>
    {
        public TSettings Settings { get; }

        private readonly List<SimpleSettingsGroup> _groups = [];

        internal SettingsGroupListBuilderImpl(TSettings settings) => Settings = settings;

        /// <inheritdoc />
        public ISettingsGroupListBuilder<TSettings> NewGroup(
            string header,
            string description = "",
            Symbol icon = default,
            Uri? descriptionUri = null,
            string? token = null,
            Action<ISettingsGroupBuilder<TSettings>> configEntries = null!,
            Action<ISettingsGroup>? config = null)
        {
            ArgumentNullException.ThrowIfNull(configEntries, $"Argument {nameof(configEntries)} should be assigned.");

            var currentGroup = new SimpleSettingsGroup(token ?? header, header, description, icon, descriptionUri);
            config?.Invoke(currentGroup);
            _groups.Add(currentGroup);

            var groupBuilder = CreateGroup(Settings);
            configEntries(groupBuilder);
            currentGroup.AddRange(groupBuilder.Build());
            return this;
        }

        /// <inheritdoc />
        public ISettingsGroupListBuilder<TSettings> NewGroup<TGroup>(
            Expression<Func<TSettings, TGroup>> property,
            Action<ISettingsGroupBuilder<TGroup>> configEntries,
            Action<ISettingsGroup>? config = null)
        {
            var currentGroup = new SimpleSettingsGroup(property);
            config?.Invoke(currentGroup);
            _groups.Add(currentGroup);

            var groupSettings = property.Compile()(Settings)
                                ?? throw new InvalidOperationException("The settings group property returned null.");
            var groupBuilder = CreateGroup(groupSettings);
            configEntries(groupBuilder);
            currentGroup.AddRange(groupBuilder.Build());
            return this;
        }

        /// <inheritdoc />
        public IReadOnlyList<ISettingsGroup> Build() => _groups;
    }
}

using System.Numerics;

namespace AutoSettingsPage.Models;

public interface IReadOnlySingleValueSettingsEntry : ISettingsEntry
{
    object Value { get; }

    string? Placeholder { get; }
}

public interface ISingleValueSettingsEntry<TValue> : IReadOnlySingleValueSettingsEntry
{
    object IReadOnlySingleValueSettingsEntry.Value => Value!;

    new TValue Value { get; set; }
}

public interface ISettingsValueReset<in TSettings>
{
    void ValueReset(TSettings settings);
}

public interface IMinMaxEntry<TValue> : ISingleValueSettingsEntry<TValue>
{
    TValue Max { get; }

    TValue Min { get; }
}

public interface INumberSettingsEntry<TNumber> : IMinMaxEntry<TNumber> where TNumber : INumberBase<TNumber>
{
    TNumber Step { get; }
}

public interface IEnumSettingsEntry<TEnum> : ISingleValueSettingsEntry<TEnum>
{
    IReadOnlyList<IReadOnlyStringPair<TEnum>> EnumItems { get; }
}

public interface IOptionSettingsEntry<out TOption> : ISettingsEntry
{
    TOption Option { get; }
}

public interface IMultiValuesSettingsEntry : ISettingsEntry
{
    IReadOnlyList<ISettingsEntry> Entries { get; }
}

public interface IMultiValuesWithMainValueSettingsEntry : IMultiValuesSettingsEntry
{
    IReadOnlySingleValueSettingsEntry MainValue { get; }
}

public interface IMultiValuesWithMainValueSettingsEntry<out TMainValue> :
    IMultiValuesWithMainValueSettingsEntry
    where TMainValue : IReadOnlySingleValueSettingsEntry
{
    new TMainValue MainValue { get; }

    IReadOnlySingleValueSettingsEntry IMultiValuesWithMainValueSettingsEntry.MainValue => MainValue;
}

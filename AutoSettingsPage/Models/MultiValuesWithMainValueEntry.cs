using System.Linq.Expressions;

namespace AutoSettingsPage.Models;

public class MultiValuesWithMainValueEntry<TSettings, TSubSettings, TMainValue>(
    TSubSettings settings,
    Expression<Func<TSettings, TSubSettings>> subSettingsProperty,
    TMainValue mainValue,
    IReadOnlyList<ISettingsEntry> entries)
    : MultiValuesEntry<TSettings, TSubSettings>(subSettingsProperty, entries),
        IMultiValuesWithMainValueSettingsEntry<TMainValue>
    where TMainValue : IReadOnlySingleValueSettingsEntry
{
    public TSubSettings Settings { get; } = settings;

    public TMainValue MainValue { get; } = mainValue;
}

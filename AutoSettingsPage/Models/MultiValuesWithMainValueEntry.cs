namespace AutoSettingsPage.Models;

public class MultiValuesWithMainValueEntry<TSettings, TMainValue>(
    TSettings settings,
    TMainValue mainValue,
    IReadOnlyList<ISettingsEntry> entries)
    : MultiValuesEntry<TSettings>(mainValue.Token,
            mainValue.Header,
            mainValue.Description,
            mainValue.Icon,
            entries,
            mainValue.DescriptionUri),
        IMultiValuesWithMainValueSettingsEntry<TMainValue>
    where TMainValue : IReadOnlySingleValueSettingsEntry
{
    public TSettings Settings { get; } = settings;

    public TMainValue MainValue { get; } = mainValue;
}

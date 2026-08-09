using AutoSettingsPage.Models;

namespace AutoSettingsPage.WinUI;

public sealed partial class MultiValuesWithSwitchSettingsExpander : IEntryControl<IMultiValuesWithMainValueSettingsEntry<ISingleValueSettingsEntry<bool>>>
{
    public IMultiValuesWithMainValueSettingsEntry<ISingleValueSettingsEntry<bool>> Entry
    {
        get;
        set
        {
            field = value;
            Items.Clear();
            foreach (var entry in Entry.Entries)
                Items.Add(SettingsEntryHelper.GetControl(entry));
        }
    } = null!;

    public MultiValuesWithSwitchSettingsExpander() => InitializeComponent();
}

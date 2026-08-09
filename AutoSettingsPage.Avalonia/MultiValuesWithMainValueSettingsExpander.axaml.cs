using AutoSettingsPage.Models;
using CommunityToolkit.Avalonia.Controls;

namespace AutoSettingsPage.Avalonia;

public partial class MultiValuesWithMainValueSettingsExpander : SettingsExpander, IEntryControl<IMultiValuesWithMainValueSettingsEntry>
{
    public IMultiValuesWithMainValueSettingsEntry Entry
    {
        set
        {
            DataContext = value;
            var mapping = SettingsEntryHelper.ResolveValueControlMapping(value.MainValue);
            Items.Clear();
            foreach (var entry in value.Entries)
                Items.Add(SettingsEntryHelper.GetChildControl(entry, value, mapping));
        }
    }

    public MultiValuesWithMainValueSettingsExpander() => InitializeComponent();
}

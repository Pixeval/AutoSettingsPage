using System.Linq.Expressions;
using FluentIcons.Common;

namespace AutoSettingsPage.Models;

public class MultiValuesEntry<TSettings> : SettingsEntryBase, IMultiValuesSettingsEntry
{
    public MultiValuesEntry(
        string token,
        string header,
        string description,
        Symbol icon,
        IReadOnlyList<ISettingsEntry> entries,
        Uri? descriptionUri = null) : base(token, header, description, icon, descriptionUri)
    {
        Entries = entries;
    }

    public MultiValuesEntry(string token, SettingsEntryAttribute attribute, IReadOnlyList<ISettingsEntry> entries)
        : base(token, attribute)
    {
        Entries = entries;
    }

    public MultiValuesEntry(Expression<Func<TSettings, object>> property, IReadOnlyList<ISettingsEntry> entries)
        : base(property)
    {
        Entries = entries;
    }

    public IReadOnlyList<ISettingsEntry> Entries { get; set; }
}

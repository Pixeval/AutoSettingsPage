using FluentIcons.Common;

namespace AutoSettingsPage;

[AttributeUsage(AttributeTargets.Property)]
public class SettingsEntryAttribute : Attribute
{
    public Symbol Icon { get; init; }

    public string Header { get; init; } = "";

    public string Description { get; init; } = "";

    public string? Placeholder { get; init; }

    public string? DescriptionLink { get; init; }

    public static SettingsEntryAttribute Empty { get; } = new();

    public SettingsEntryAttribute()
    {
    }

    public SettingsEntryAttribute(
        Symbol icon,
        string? headerResource,
        string? descriptionResource,
        string? placeholderResource = null,
        string? descriptionLinkResource = null)
    {
        Icon = icon;
        if (headerResource is not null)
            Header = SettingsResourceKeysProvider[headerResource];
        if (descriptionResource is not null)
            Description = SettingsResourceKeysProvider[descriptionResource];
        if (placeholderResource is not null)
            Placeholder = SettingsResourceKeysProvider[placeholderResource];
        if (descriptionLinkResource is not null)
            DescriptionLink = SettingsResourceKeysProvider[descriptionLinkResource];
    }

    public static ISettingsResourceKeysProvider SettingsResourceKeysProvider { get; set; } = SimpleSettingsResourceKeysProvider.Default;

    internal Uri? DescriptionUri => string.IsNullOrWhiteSpace(DescriptionLink)
        ? null
        : new Uri(DescriptionLink, UriKind.RelativeOrAbsolute);
}

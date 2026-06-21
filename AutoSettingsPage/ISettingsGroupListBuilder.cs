using System.Linq.Expressions;
using AutoSettingsPage.Models;
using FluentIcons.Common;

namespace AutoSettingsPage;

public interface ISettingsGroupListBuilder<TSettings>
{
    TSettings Settings { get; }

    ISettingsGroupListBuilder<TSettings> NewGroup(
        string header,
        string description = "",
        Symbol icon = default,
        Uri? descriptionUri = null,
        string? token = null,
        Action<ISettingsGroupBuilder<TSettings>> configEntries = null!,
        Action<ISettingsGroup>? config = null);

    ISettingsGroupListBuilder<TSettings> NewGroup<TGroup>(
        Expression<Func<TSettings, TGroup>> property,
        Action<ISettingsGroupBuilder<TGroup>> configEntries,
        Action<ISettingsGroup>? config = null);

    IReadOnlyList<ISettingsGroup> Build();
}

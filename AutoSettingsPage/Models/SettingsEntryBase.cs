using System.Linq.Expressions;
using System.Reflection;
using FluentIcons.Common;

namespace AutoSettingsPage.Models;

public abstract class SettingsEntryBase(
    string token,
    string header,
    string description,
    Symbol icon,
    Uri? descriptionUri = null) : ISettingsEntry
{
    protected SettingsEntryBase(string token, SettingsEntryAttribute attribute)
        : this(token, attribute.Header, attribute.Description, attribute.Icon, attribute.DescriptionUri)
    {
    }

    /// <remarks>
    /// Only extract <see cref="SettingsEntryAttribute"/> from <paramref name="propertyExpression"/>
    /// </remarks>
    protected SettingsEntryBase(LambdaExpression propertyExpression)
        : this(GetMemberAttribute(propertyExpression, out _, out var attribute), attribute)
    {
    }

    public string Token { get; } = token;

    public string Header { get; set; } = header;

    public string Description { get; set; } = description;

    public Symbol Icon { get; set; } = icon;

    public virtual Uri? DescriptionUri { get; set; } = descriptionUri;

    private protected static string GetMemberAttribute(LambdaExpression propertyExpression, out MemberExpression member, out SettingsEntryAttribute attribute)
    {
        member = propertyExpression.Body switch
        {
            // t => (T)t.A
            UnaryExpression { Operand: MemberExpression member1 } => member1,
            // t => t.A
            MemberExpression member2 => member2,
            _ => throw new ArgumentException(PropertyExceptionString, nameof(propertyExpression))
        };
        attribute = member.Member.GetCustomAttribute<SettingsEntryAttribute>() ?? SettingsEntryAttribute.Empty;
        return member.Member.Name;
    }

    private protected const string PropertyExceptionString = "The property expression is not a valid member expression.";
}

using Singulink.Enums;

namespace Singulink.Cryptography;

public class ContextualSubject
{
    public ContextualSubjectType Type { get; }

    public string Value { get; }

    public ContextualSubject(string value, ContextualSubjectType type = ContextualSubjectType.General)
    {
        type.ThrowIfNotDefined(nameof(type));

        Value = value?.Trim() ?? string.Empty;
        Type = type;
    }
}

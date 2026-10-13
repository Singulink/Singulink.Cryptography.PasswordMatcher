using Singulink.Enums;

namespace Singulink.Cryptography;

/// <summary>
/// A value specific to the user or application, such as the user's name or the application name, that a password should not be composed of.
/// </summary>
public class ContextualSubject
{
    /// <summary>
    /// Gets the type of the subject, which determines how the value is interpreted.
    /// </summary>
    public ContextualSubjectType Type { get; }

    /// <summary>
    /// Gets the trimmed subject value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ContextualSubject"/> class.
    /// </summary>
    /// <param name="value">The subject value, such as a name, email address or website.</param>
    /// <param name="type">The type of the subject.</param>
    public ContextualSubject(string value, ContextualSubjectType type = ContextualSubjectType.General)
    {
        type.ThrowIfNotDefined(nameof(type));

        Value = value.Trim();
        Type = type;
    }
}

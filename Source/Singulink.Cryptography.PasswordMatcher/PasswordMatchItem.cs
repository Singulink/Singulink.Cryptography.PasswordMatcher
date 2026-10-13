using Singulink.Enums;

namespace Singulink.Cryptography;

/// <summary>
/// A value that was matched within a password.
/// </summary>
public record PasswordMatchItem
{
    /// <summary>
    /// Gets the value that was matched, in the form it should be displayed to the user (for example, the dictionary word rather than the
    /// substituted characters that were actually present in the password).
    /// </summary>
    public string MatchedText { get; }

    /// <summary>
    /// Gets the characters of the normalized password (see <see cref="PasswordMatchContext.CheckedPassword"/>) that were matched, for example
    /// "p@55w0rd" for a <see cref="MatchedText"/> of "password". Useful for highlighting the matched part of what the user typed.
    /// </summary>
    public string MatchedChars { get; private init; }

    /// <summary>
    /// Gets the type of match.
    /// </summary>
    public PasswordMatchType MatchType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PasswordMatchItem"/> class. <see cref="MatchedChars"/> is set automatically when the item is
    /// passed to <see cref="PasswordMatchContext.CreateChild(int, PasswordMatchItem)"/>.
    /// </summary>
    /// <param name="matchedText">The value that was matched, in the form it should be displayed to the user.</param>
    /// <param name="matchType">The type of match.</param>
    public PasswordMatchItem(string matchedText, PasswordMatchType matchType)
    {
        if (string.IsNullOrEmpty(matchedText))
            throw new ArgumentException("Matched text cannot be null or empty.", nameof(matchedText));

        matchType.ThrowIfNotDefined(nameof(matchType));

        MatchedText = matchedText;
        MatchedChars = matchedText;
        MatchType = matchType;
    }

    /// <summary>
    /// Returns a copy of this item with the given matched characters.
    /// </summary>
    internal PasswordMatchItem WithMatchedChars(string matchedChars)
    {
        return MatchedChars == matchedChars ? this : this with { MatchedChars = matchedChars };
    }

    /// <summary>
    /// Returns the matched text and match type.
    /// </summary>
    public override string ToString() => $"\"{MatchedText}\" ({MatchType})";
}

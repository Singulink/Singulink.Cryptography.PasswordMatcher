using Singulink.Enums;

namespace Singulink.Cryptography;

public record PasswordMatchItem
{
    public string MatchedText { get; }

    public PasswordMatchType MatchType { get; }

    public PasswordMatchItem(string matchedText, PasswordMatchType matchType)
    {
        if (string.IsNullOrWhiteSpace(matchedText))
            throw new ArgumentException("Matched text cannot be null or empty.", nameof(matchedText));

        matchType.ThrowIfNotDefined(nameof(matchType));

        MatchedText = matchedText;
        MatchType = matchType;
    }

    public override string ToString() => $"\"{MatchedText}\" ({MatchType})";
}
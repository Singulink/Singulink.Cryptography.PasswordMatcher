namespace Singulink.Cryptography;

public class PasswordMatchResult
{
    public required bool Matched { get; init; }

    public required IReadOnlyList<string> MatchedValues { get; init; }
}

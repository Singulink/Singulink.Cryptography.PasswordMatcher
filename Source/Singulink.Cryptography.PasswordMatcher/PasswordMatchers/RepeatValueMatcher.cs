using System.Diagnostics;

namespace Singulink.Cryptography.PasswordMatchers;

internal sealed class RepeatValueMatcher : ValueMatcher
{
    public ValueMatcher ParentMatcher { get; }

    public string Value { get; }

    public string? MatchedText { get; }

    public RepeatValueMatcher(ValueMatcher parentMatcher, string value, string? matchedText, bool matchTrailingSeparator)
        : base(true, matchTrailingSeparator)
    {
#pragma warning disable CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons
        Debug.Assert(value.Trim().ToLowerInvariant() == value, "received non-normalized value");
#pragma warning restore CA1862

        if (value.Length is 0)
            throw new UnreachableException("Value should not be empty.");

        ParentMatcher = parentMatcher;
        Value = value;
        MatchedText = matchedText;
    }

    protected override IEnumerable<PasswordMatchContext> GetValueMatches(PasswordMatchContext context)
    {
        if (!context.RemainingChars.StartsWith(Value, StringComparison.Ordinal))
            return [];

        var matchedItem = MatchedText is not null ? new PasswordMatchItem(MatchedText, PasswordMatchType.RepeatValue) : null;

        return [context.CreateChild(ParentMatcher, Value.Length, matchedItem)];
    }
}

<div class="article">

# Writing Custom Matchers

The factory methods cover words, sequences, alternatives, permutations, keyboard runs, digits and repeated characters. When a pattern needs something else, such as a large word list or a structure the built-in matchers cannot express, write a matcher of your own. This guide covers the two base classes, the match context they work with, and how to test a matcher.

### Two base classes

A matcher that consumes characters from the password derives from <xref:Singulink.Cryptography.PasswordMatchers.ValueMatcher>, which handles repeats and trailing separators so that the matcher only has to recognize its value. A matcher that composes other matchers derives from <xref:Singulink.Cryptography.PasswordMatcher> directly and calls <xref:Singulink.Cryptography.PasswordMatcher.GetMatches*> on its children. Both are immutable once constructed and can be shared freely.

## The Match Context

Every matcher works with a <xref:Singulink.Cryptography.PasswordMatchContext>, which represents a position in the password together with the items matched so far. The members a matcher uses are:

| Member | Purpose |
|---|---|
| <xref:Singulink.Cryptography.PasswordMatchContext.RemainingChars> | The characters from the current position to the end of the password. This is what a matcher inspects. |
| <xref:Singulink.Cryptography.PasswordMatchContext.CheckedPassword> | The whole normalized password: trimmed, lower-cased, with runs of spaces collapsed to one. |
| <xref:Singulink.Cryptography.PasswordMatchContext.TotalMatchedLength> | The current position, i.e. how many characters have been matched so far. |
| <xref:Singulink.Cryptography.PasswordMatchContext.CreateChild*> | Creates the context that results from matching a number of characters at the current position, with the <xref:Singulink.Cryptography.PasswordMatchItem> to report (or `null` for characters that should be consumed without being reported, such as separators). |

Contexts are immutable. A matcher never advances a context; it creates child contexts for each way it can match and returns them.

> [!IMPORTANT]
> A matcher's results must depend only on <xref:Singulink.Cryptography.PasswordMatchContext.RemainingChars> (and the matcher's own configuration). Results are cached per matcher and position for the duration of a check, so a matcher that looked at what was matched before it would get stale answers. The context deliberately exposes nothing about earlier matches.

## Value Matchers

Derive from <xref:Singulink.Cryptography.PasswordMatchers.ValueMatcher> and implement <xref:Singulink.Cryptography.PasswordMatchers.ValueMatcher.GetValueMatches*>, which returns a child context for each way the value can match at the current position, longest first. The base class takes care of the rest: when <xref:Singulink.Cryptography.PasswordMatchers.ValueMatcher.MatchRepeats> is `true` it also matches consecutive repeats of the exact characters that were matched, and when <xref:Singulink.Cryptography.PasswordMatchers.ValueMatcher.MatchTrailingSeparator> is `true` it also matches a single separator character after each occurrence.

This matcher recognizes any word from a large list, which is more efficient than an <xref:Singulink.Cryptography.PasswordMatchers.AnyMatcher> over thousands of text matchers because it looks the candidate up instead of trying every word:

```csharp
public sealed class WordListMatcher : ValueMatcher
{
    private readonly HashSet<string> _words;
    private readonly int _minLength;
    private readonly int _maxLength;

    public WordListMatcher(IEnumerable<string> words, bool matchRepeats = true, bool matchTrailingSeparator = true)
        : base(matchRepeats, matchTrailingSeparator)
    {
        _words = words.Select(w => w.Trim().ToLowerInvariant()).Where(w => w.Length > 0).ToHashSet();
        _minLength = _words.Min(w => w.Length);
        _maxLength = _words.Max(w => w.Length);
    }

    protected override IEnumerable<PasswordMatchContext> GetValueMatches(PasswordMatchContext context)
    {
        var remaining = context.RemainingChars;
        int maxLength = Math.Min(_maxLength, remaining.Length);
        var results = new List<PasswordMatchContext>();

        // Longest first, so that "password" is preferred over "pass" when both are in the list.

        for (int length = maxLength; length >= _minLength; length--)
        {
            string candidate = remaining[..length].ToString();

            if (_words.Contains(candidate))
                results.Add(context.CreateChild(length, new PasswordMatchItem(candidate, PasswordMatchType.Common)));
        }

        return results;
    }
}
```

Every result is produced with <xref:Singulink.Cryptography.PasswordMatchContext.CreateChild*>, which validates that the length is within the remaining characters. The <xref:Singulink.Cryptography.PasswordMatchItem> carries the text to show the user and the <xref:Singulink.Cryptography.PasswordMatchType> to report; use <xref:Singulink.Cryptography.PasswordMatchType.ContextualSubject> for values derived from the user's details and <xref:Singulink.Cryptography.PasswordMatchType.Common> for everything else.

> [!NOTE]
> <xref:Singulink.Cryptography.PasswordMatchContext.RemainingChars> is a <xref:System.ReadOnlySpan`1>, which cannot be held across a `yield return`. A value matcher that returns several results therefore collects them in a list, as the example does, rather than being written as an iterator. Composite matchers, which only deal in contexts, can use iterators freely.

#### Returning several lengths

A value matcher can match at more than one length, as the example does. Return the lengths in the order they should be tried: longest first is almost always right, since it keeps the reported match whole and lets the following matchers see the rest of the password. The results for a matcher are deduplicated by end position, so returning the same length twice is harmless but pointless.

## Composite Matchers

Derive from <xref:Singulink.Cryptography.PasswordMatcher> and implement <xref:Singulink.Cryptography.PasswordMatcher.GetMatchesCore*> to combine other matchers. Call <xref:Singulink.Cryptography.PasswordMatcher.GetMatches*> on each child to get its results for a context, then return the contexts your composition produces. This matcher limits how many characters its inner matcher may consume, which is useful for keeping a greedy matcher (such as a word list that happens to contain very long entries) from accounting for more of the password than intended:

```csharp
public sealed class MaxLengthMatcher : PasswordMatcher
{
    public PasswordMatcher Matcher { get; }

    public int MaxLength { get; }

    public MaxLengthMatcher(PasswordMatcher matcher, int maxLength)
    {
        if (maxLength < 1)
            throw new ArgumentOutOfRangeException(nameof(maxLength));

        Matcher = matcher;
        MaxLength = maxLength;
    }

    protected override IEnumerable<PasswordMatchContext> GetMatchesCore(PasswordMatchContext context)
    {
        foreach (var match in Matcher.GetMatches(context))
        {
            if (match.TotalMatchedLength - context.TotalMatchedLength <= MaxLength)
                yield return match;
        }
    }
}
```

The order in which a composite returns its results is the order in which they are tried by whatever contains it, and the first complete match of the whole password is the one reported. The example preserves the inner matcher's order. A composite that explores combinations, such as the built-in <xref:Singulink.Cryptography.PasswordMatchers.SequenceMatcher> and <xref:Singulink.Cryptography.PasswordMatchers.RepeatMatcher>, should keep only the first result per end position after each step rather than enumerating every combination, since the number of combinations can grow very quickly on long passwords even though the set of end positions cannot.

#### What the base class does

<xref:Singulink.Cryptography.PasswordMatcher.GetMatches*> is not virtual: it wraps <xref:Singulink.Cryptography.PasswordMatcher.GetMatchesCore*>, caches the results per matcher and position for the duration of the check, and keeps only the first result for each distinct end position. A composite therefore never needs to worry about calling a child repeatedly for the same position, and never needs to deduplicate its own results. See [How Matching Works](../concepts/how-matching-works.md) for why this is what keeps matching fast.

## Testing a Matcher

<xref:Singulink.Cryptography.PasswordMatcher.GetFirstMatch*> runs a password against a list of matchers and returns a match only when the matchers consume the whole password, which makes it a precise way to test a matcher in isolation:

```csharp
var matcher = new WordListMatcher(["correct", "horse", "battery", "staple"]);
var phrase = PasswordMatcher.Repeat(matcher, 1, 4);

PasswordMatcher.GetFirstMatch("correcthorsebatterystaple", [phrase])!.Values;  // ["correct", "horse", "battery", "staple"]
PasswordMatcher.GetFirstMatch("horse.horse", [phrase])!.Values;                 // ["horse", "horse"]
PasswordMatcher.GetFirstMatch("horses", [phrase]);                               // null
```

Compose the matcher into a pattern and register it through a matchers provider to use it in real checks. See [Customizing the Patterns](customizing-patterns.md).

## Next Steps

Continue with these related articles:

- [Customizing the Patterns](customizing-patterns.md) - Plugging a custom matcher into the patterns.
- [How Matching Works](../concepts/how-matching-works.md) - Normalization, ordering, caching and the rules every matcher follows.
- [Match Results](match-results.md) - How the items a matcher creates are reported.

</div>

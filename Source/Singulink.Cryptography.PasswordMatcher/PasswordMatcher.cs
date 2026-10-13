using Singulink.Cryptography.PasswordMatchers;

namespace Singulink.Cryptography;

/// <summary>
/// Base class for password matchers, which test whether the characters at the current position of a password follow a particular pattern.
/// </summary>
/// <remarks>
/// <para>Matchers are composed into a grammar with <see cref="Sequence(IEnumerable{PasswordMatcher})"/>, <see cref="Any(IEnumerable{PasswordMatcher})"/>,
/// <see cref="Optional(PasswordMatcher)"/>, <see cref="Repeat(PasswordMatcher, int, int)"/> and
/// <see cref="Permutations(IEnumerable{PasswordMatcher}, bool)"/>. A password matches when the grammar consumes it entirely.</para>
/// <para>Custom matchers that consume text should derive from <see cref="ValueMatcher"/>. Custom matchers that compose other matchers can derive
/// from this class directly and call <see cref="GetMatches(PasswordMatchContext)"/> on their child matchers.</para>
/// <para>The results a matcher produces for a context must depend only on the remaining characters of the password, not on the matches that
/// preceded it, because results are cached per matcher and position for the duration of a password check. Matchers are immutable and safe to share
/// between threads.</para>
/// </remarks>
public abstract class PasswordMatcher
{
    /// <summary>
    /// Gets the contexts that result from matching this matcher at the current position of the given context.
    /// </summary>
    /// <remarks>
    /// <para>Results are cached per matcher and position for the duration of a password check, and only the first result for each distinct end
    /// position is retained. Results are returned in the order they were produced by <see cref="GetMatchesCore(PasswordMatchContext)"/>.</para>
    /// </remarks>
    /// <param name="context">The context to match against.</param>
    /// <returns>The contexts that result from each match, or an empty list if there were no matches.</returns>
    public IReadOnlyList<PasswordMatchContext> GetMatches(PasswordMatchContext context)
    {
        var cache = context.Cache;

        if (cache.TryGet(this, context.TotalMatchedLength, out var entry))
        {
            if (ReferenceEquals(entry.Root, context))
                return entry.Results;

            var spliced = new List<PasswordMatchContext>(entry.Results.Count);

            foreach (var result in entry.Results)
                spliced.Add(context.Splice(entry.Root, result));

            return spliced;
        }

        var results = new List<PasswordMatchContext>();
        HashSet<int>? seenEndPositions = null;

        foreach (var match in GetMatchesCore(context))
        {
            if (results.Count is 0)
            {
                results.Add(match);
                continue;
            }

            seenEndPositions ??= [results[0].TotalMatchedLength];

            if (seenEndPositions.Add(match.TotalMatchedLength))
                results.Add(match);
        }

        cache.Add(this, context, results);
        return results;
    }

    /// <summary>
    /// Gets the first match for a password from the given matchers, or <see langword="null"/> if the password does not match any of them.
    /// </summary>
    /// <remarks>
    /// <para>A matcher only matches the password if it consumes the entire password. Matchers are tried in order and the first match found is
    /// returned.</para>
    /// </remarks>
    /// <param name="password">The password to match.</param>
    /// <param name="matchers">The matchers to try, in order.</param>
    public static PasswordMatch? GetFirstMatch(string password, IEnumerable<PasswordMatcher> matchers)
    {
        var context = new PasswordMatchContext(password);

        foreach (var matcher in matchers)
        {
            foreach (var c in matcher.GetMatches(context))
            {
                if (c.TotalMatchedLength == c.CheckedPassword.Length)
                    return new PasswordMatch(c.GetItems());
            }
        }

        return null;
    }

    /// <summary>
    /// Creates a matcher that matches any one of the given matchers.
    /// </summary>
    /// <param name="matchers">The matchers to try, in order. Optional matchers are not permitted.</param>
    public static PasswordMatcher Any(IEnumerable<PasswordMatcher> matchers)
    {
        return new AnyMatcher(matchers);
    }

    /// <summary>
    /// Creates a matcher that matches a short run of arbitrary characters, optionally limited to characters that pass a filter.
    /// </summary>
    /// <param name="minLength">The minimum number of characters to match.</param>
    /// <param name="maxLength">The maximum number of characters to match.</param>
    /// <param name="charFilter">A filter that determines which characters can be matched, or <see langword="null"/> to match any character.</param>
    /// <param name="matchTrailingSeparator">Whether a single separator character after the characters is also matched.</param>
    public static PasswordMatcher AnyChars(int minLength, int maxLength, Func<char, bool>? charFilter, bool matchTrailingSeparator = true)
    {
        return new AnyCharsMatcher(minLength, maxLength, charFilter, matchTrailingSeparator);
    }

    /// <summary>
    /// Creates a matcher that matches sequences of characters that are adjacent on a keyboard or keypad or consecutive in the alphabet, such as
    /// "qwerty", "zaq1", "!@#$", "1q2w3e4r", "7410" or "abcdef".
    /// </summary>
    /// <param name="sequenceTypes">The types of sequences to match.</param>
    /// <param name="matchTrailingSeparator">Whether a single separator character after the sequence is also matched.</param>
    public static PasswordMatcher CharSequence(CharSequenceTypes sequenceTypes, bool matchTrailingSeparator = true)
    {
        return new CharSequenceMatcher(sequenceTypes, matchTrailingSeparator);
    }

    /// <summary>
    /// Creates a matcher that matches a fixed number of digits.
    /// </summary>
    /// <param name="numDigits">The number of digits to match (between 1 and 9).</param>
    /// <param name="matchType">The type of match to report.</param>
    /// <param name="matchRepeats">Whether consecutive repeats of the matched digits are also matched.</param>
    /// <param name="matchTrailingSeparator">Whether a single separator character after the digits is also matched.</param>
    public static PasswordMatcher FixedDigits(int numDigits, PasswordMatchType matchType, bool matchRepeats = true, bool matchTrailingSeparator = true)
    {
        return new FixedDigitsMatcher(numDigits, minValue: 0, maxValue: (int)Math.Pow(10, numDigits) - 1, matchType, matchRepeats, matchTrailingSeparator);
    }

    /// <summary>
    /// Creates a matcher that matches a fixed number of digits whose value falls within a range.
    /// </summary>
    /// <param name="numDigits">The number of digits to match (between 1 and 9).</param>
    /// <param name="minValue">The minimum value of the digits (inclusive).</param>
    /// <param name="maxValue">The maximum value of the digits (inclusive).</param>
    /// <param name="matchType">The type of match to report.</param>
    /// <param name="matchRepeats">Whether consecutive repeats of the matched digits are also matched.</param>
    /// <param name="matchTrailingSeparator">Whether a single separator character after the digits is also matched.</param>
    public static PasswordMatcher FixedDigits(int numDigits, int minValue, int maxValue, PasswordMatchType matchType, bool matchRepeats = true, bool matchTrailingSeparator = true)
    {
        return new FixedDigitsMatcher(numDigits, minValue, maxValue, matchType, matchRepeats, matchTrailingSeparator);
    }

    /// <summary>
    /// Creates a matcher that matches the given matcher or nothing at all.
    /// </summary>
    /// <param name="matcher">The matcher to make optional.</param>
    public static PasswordMatcher Optional(PasswordMatcher matcher)
    {
        return new OptionalMatcher(matcher);
    }

    /// <summary>
    /// Creates a matcher that matches the given matchers in any order, with each matcher used at most once.
    /// </summary>
    /// <param name="matchers">The matchers to permute. Each must be a distinct instance, and optional matchers are not permitted.</param>
    /// <param name="mustMatchAllMatchers">Whether every matcher must match for the permutation to match. If <see langword="false"/>, any
    /// non-empty subset of the matchers can match.</param>
    public static PasswordMatcher Permutations(IEnumerable<PasswordMatcher> matchers, bool mustMatchAllMatchers = false)
    {
        return new PermutationsMatcher(matchers, mustMatchAllMatchers);
    }

    /// <summary>
    /// Creates a matcher that matches the given matcher a number of times in a row, within a range.
    /// </summary>
    /// <param name="matcher">The matcher to repeat. Optional matchers are not permitted.</param>
    /// <param name="minCount">The minimum number of occurrences to match (at least 1).</param>
    /// <param name="maxCount">The maximum number of occurrences to match.</param>
    public static PasswordMatcher Repeat(PasswordMatcher matcher, int minCount, int maxCount)
    {
        return new RepeatMatcher(matcher, minCount, maxCount);
    }

    /// <summary>
    /// Creates a matcher that matches one or more consecutive occurrences of the same character.
    /// </summary>
    /// <param name="charFilter">A filter that determines which characters can be matched, or <see langword="null"/> to match any character.</param>
    /// <param name="matchTrailingSeparator">Whether a single separator character after the repeated characters is also matched.</param>
    public static PasswordMatcher RepeatedChars(Func<char, bool>? charFilter, bool matchTrailingSeparator = true)
    {
        return new RepeatedCharMatcher(1, int.MaxValue, charFilter, matchTrailingSeparator);
    }

    /// <summary>
    /// Creates a matcher that matches between <paramref name="minCount"/> and <paramref name="maxCount"/> consecutive occurrences of the same
    /// character.
    /// </summary>
    /// <param name="minCount">The minimum number of repeated characters to match.</param>
    /// <param name="maxCount">The maximum number of repeated characters to match.</param>
    /// <param name="charFilter">A filter that determines which characters can be matched, or <see langword="null"/> to match any character.</param>
    /// <param name="matchTrailingSeparator">Whether a single separator character after the repeated characters is also matched.</param>
    public static PasswordMatcher RepeatedChars(int minCount, int maxCount, Func<char, bool>? charFilter, bool matchTrailingSeparator = true)
    {
        return new RepeatedCharMatcher(minCount, maxCount, charFilter, matchTrailingSeparator);
    }

    /// <summary>
    /// Creates a matcher that matches a short group of characters repeated consecutively, such as "121212" or "abab".
    /// </summary>
    /// <param name="minGroupLength">The minimum length of the repeated group (at least 2).</param>
    /// <param name="maxGroupLength">The maximum length of the repeated group.</param>
    /// <param name="minRepeats">The minimum number of times the group must occur (at least 2).</param>
    /// <param name="charFilter">A filter that determines which characters the group can contain, or <see langword="null"/> to allow any
    /// character.</param>
    /// <param name="matchTrailingSeparator">Whether a single separator character after the repeated group is also matched.</param>
    public static PasswordMatcher RepeatedGroups(int minGroupLength, int maxGroupLength, int minRepeats, Func<char, bool>? charFilter, bool matchTrailingSeparator = true)
    {
        return new RepeatedGroupMatcher(minGroupLength, maxGroupLength, minRepeats, charFilter, matchTrailingSeparator);
    }

    /// <summary>
    /// Creates a matcher that matches a word or phrase, ignoring case and optionally allowing common character substitutions (such as "@" for "a").
    /// </summary>
    /// <param name="text">The text to match.</param>
    /// <param name="matchType">The type of match to report.</param>
    /// <param name="checkSubstitutions">Whether common character substitutions are matched in place of the original characters.</param>
    /// <param name="matchRepeats">Whether consecutive repeats of the matched text are also matched.</param>
    /// <param name="matchTrailingSeparator">Whether a single separator character after the text is also matched.</param>
    public static PasswordMatcher Text(string text, PasswordMatchType matchType = PasswordMatchType.Common, bool checkSubstitutions = true, bool matchRepeats = true, bool matchTrailingSeparator = true)
    {
        return new TextMatcher(text, matchType, checkSubstitutions, matchRepeats, matchTrailingSeparator);
    }

    /// <summary>
    /// Creates a matcher that matches the given matchers one after another, in order.
    /// </summary>
    /// <param name="matchers">The matchers to match in sequence. At least one must not be optional.</param>
    public static PasswordMatcher Sequence(IEnumerable<PasswordMatcher> matchers)
    {
        return new SequenceMatcher(matchers);
    }

    /// <summary>
    /// Gets the contexts that result from matching this matcher at the current position of the given context, in the order they should be tried.
    /// </summary>
    /// <remarks>
    /// <para>Implementations should produce results lazily where practical, and must only produce results that depend on the remaining characters of
    /// the password. Results are created with <see cref="PasswordMatchContext.CreateChild(int, PasswordMatchItem)"/>.</para>
    /// </remarks>
    /// <param name="context">The context to match against.</param>
    protected abstract IEnumerable<PasswordMatchContext> GetMatchesCore(PasswordMatchContext context);
}

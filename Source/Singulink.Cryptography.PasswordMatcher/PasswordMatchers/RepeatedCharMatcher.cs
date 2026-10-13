namespace Singulink.Cryptography.PasswordMatchers;

/// <summary>
/// Matcher that matches consecutive occurrences of the same character, such as "111" or "!!!".
/// </summary>
public sealed class RepeatedCharMatcher : ValueMatcher
{
    /// <summary>
    /// Gets the minimum number of repeated characters that are matched.
    /// </summary>
    public int MinCount { get; }

    /// <summary>
    /// Gets the maximum number of repeated characters that are matched.
    /// </summary>
    public int MaxCount { get; }

    /// <summary>
    /// Gets the filter that determines which characters can be matched, or <see langword="null"/> if any character can be matched.
    /// </summary>
    public Func<char, bool>? CharFilter { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RepeatedCharMatcher"/> class.
    /// </summary>
    /// <param name="minCount">The minimum number of repeated characters to match.</param>
    /// <param name="maxCount">The maximum number of repeated characters to match.</param>
    /// <param name="charFilter">A filter that determines which characters can be matched, or <see langword="null"/> to match any character.</param>
    /// <param name="matchTrailingSeparator">Whether a single separator character after the repeated characters is also matched.</param>
    public RepeatedCharMatcher(int minCount, int maxCount, Func<char, bool>? charFilter, bool matchTrailingSeparator)
        : base(matchRepeats: false, matchTrailingSeparator)
    {
        if (minCount < 1)
            throw new ArgumentOutOfRangeException(nameof(minCount), "Minimum count must be at least 1.");

        if (maxCount < minCount)
            throw new ArgumentOutOfRangeException(nameof(maxCount), "Maximum count must be greater than or equal to minimum count.");

        MinCount = minCount;
        MaxCount = maxCount;
        CharFilter = charFilter;
    }

    /// <inheritdoc/>
    protected override IEnumerable<PasswordMatchContext> GetValueMatches(PasswordMatchContext context)
    {
        var p = context.RemainingChars;

        if (p.Length < MinCount)
            return [];

        char c = p[0];

        if (CharFilter is not null && !CharFilter(c))
            return [];

        int count = 1;

        while (count < p.Length && count < MaxCount && p[count] == c)
            count++;

        if (count < MinCount)
            return [];

        // Longest match first

        var results = new List<PasswordMatchContext>(count - MinCount + 1);

        for (int length = count; length >= MinCount; length--)
            results.Add(context.CreateChild(length, new PasswordMatchItem(p[..length].ToString(), PasswordMatchType.RepeatedChar)));

        return results;
    }
}

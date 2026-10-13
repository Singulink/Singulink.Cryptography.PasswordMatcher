namespace Singulink.Cryptography.PasswordMatchers;

/// <summary>
/// Matcher that matches a short run of arbitrary characters, optionally limited to characters that pass a filter, such as a few digits or symbols
/// appended to a word.
/// </summary>
/// <remarks>
/// <para>Unlike other value matchers, the shortest match is produced first, so that as few arbitrary characters as possible are consumed and
/// structured matchers (sequences, runs, years and words) get the chance to account for the rest.</para>
/// </remarks>
public sealed class AnyCharsMatcher : ValueMatcher
{
    /// <summary>
    /// Gets the minimum number of characters that are matched.
    /// </summary>
    public int MinLength { get; }

    /// <summary>
    /// Gets the maximum number of characters that are matched.
    /// </summary>
    public int MaxLength { get; }

    /// <summary>
    /// Gets the filter that determines which characters can be matched, or <see langword="null"/> if any character can be matched.
    /// </summary>
    public Func<char, bool>? CharFilter { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AnyCharsMatcher"/> class.
    /// </summary>
    /// <param name="minLength">The minimum number of characters to match (at least 1).</param>
    /// <param name="maxLength">The maximum number of characters to match.</param>
    /// <param name="charFilter">A filter that determines which characters can be matched, or <see langword="null"/> to match any character.</param>
    /// <param name="matchTrailingSeparator">Whether a single separator character after the characters is also matched.</param>
    public AnyCharsMatcher(int minLength, int maxLength, Func<char, bool>? charFilter, bool matchTrailingSeparator)
        : base(matchRepeats: false, matchTrailingSeparator)
    {
        if (minLength < 1)
            throw new ArgumentOutOfRangeException(nameof(minLength), "Minimum length must be at least 1.");

        if (maxLength < minLength)
            throw new ArgumentOutOfRangeException(nameof(maxLength), "Maximum length must be greater than or equal to minimum length.");

        MinLength = minLength;
        MaxLength = maxLength;
        CharFilter = charFilter;
    }

    /// <inheritdoc/>
    protected override IEnumerable<PasswordMatchContext> GetValueMatches(PasswordMatchContext context)
    {
        var p = context.RemainingChars;
        int count = 0;

        while (count < p.Length && count < MaxLength && (CharFilter is null || CharFilter(p[count])))
            count++;

        if (count < MinLength)
            return [];

        // Shortest first

        var results = new List<PasswordMatchContext>(count - MinLength + 1);

        for (int length = MinLength; length <= count; length++)
            results.Add(context.CreateChild(length, new PasswordMatchItem(p[..length].ToString(), PasswordMatchType.AnyChars)));

        return results;
    }
}

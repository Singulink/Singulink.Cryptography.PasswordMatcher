namespace Singulink.Cryptography.PasswordMatchers;

/// <summary>
/// Matcher that matches a short group of characters repeated consecutively, such as "121212", "abab" or "1!1!1!".
/// </summary>
/// <remarks>
/// <para>Groups consisting of a single repeated character (such as "11") are not matched, since those are runs of a repeated character and are
/// matched by <see cref="RepeatedCharMatcher"/>. The longest match is produced first.</para>
/// </remarks>
public sealed class RepeatedGroupMatcher : ValueMatcher
{
    /// <summary>
    /// Gets the minimum length of the repeated group.
    /// </summary>
    public int MinGroupLength { get; }

    /// <summary>
    /// Gets the maximum length of the repeated group.
    /// </summary>
    public int MaxGroupLength { get; }

    /// <summary>
    /// Gets the minimum number of times the group must occur.
    /// </summary>
    public int MinRepeats { get; }

    /// <summary>
    /// Gets the filter that determines which characters the group can contain, or <see langword="null"/> if any character is allowed.
    /// </summary>
    public Func<char, bool>? CharFilter { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RepeatedGroupMatcher"/> class.
    /// </summary>
    /// <param name="minGroupLength">The minimum length of the repeated group (at least 2).</param>
    /// <param name="maxGroupLength">The maximum length of the repeated group.</param>
    /// <param name="minRepeats">The minimum number of times the group must occur (at least 2).</param>
    /// <param name="charFilter">A filter that determines which characters the group can contain, or <see langword="null"/> to allow any
    /// character.</param>
    /// <param name="matchTrailingSeparator">Whether a single separator character after the repeated group is also matched.</param>
    public RepeatedGroupMatcher(int minGroupLength, int maxGroupLength, int minRepeats, Func<char, bool>? charFilter, bool matchTrailingSeparator)
        : base(matchRepeats: false, matchTrailingSeparator)
    {
        if (minGroupLength < 2)
            throw new ArgumentOutOfRangeException(nameof(minGroupLength), "Minimum group length must be at least 2.");

        if (maxGroupLength < minGroupLength)
            throw new ArgumentOutOfRangeException(nameof(maxGroupLength), "Maximum group length must be greater than or equal to minimum group length.");

        if (minRepeats < 2)
            throw new ArgumentOutOfRangeException(nameof(minRepeats), "Minimum repeats must be at least 2.");

        MinGroupLength = minGroupLength;
        MaxGroupLength = maxGroupLength;
        MinRepeats = minRepeats;
        CharFilter = charFilter;
    }

    /// <inheritdoc/>
    protected override IEnumerable<PasswordMatchContext> GetValueMatches(PasswordMatchContext context)
    {
        var p = context.RemainingChars;
        var results = new List<PasswordMatchContext>();

        for (int groupLength = MinGroupLength; groupLength <= MaxGroupLength; groupLength++)
        {
            if (p.Length < groupLength * MinRepeats)
                break;

            var group = p[..groupLength];
            bool allSameChar = true;
            bool allowed = true;

            for (int i = 0; i < groupLength; i++)
            {
                if (CharFilter is not null && !CharFilter(group[i]))
                    allowed = false;

                if (group[i] != group[0])
                    allSameChar = false;
            }

            if (!allowed || allSameChar)
                continue;

            int count = 1;

            while ((count + 1) * groupLength <= p.Length && p.Slice(count * groupLength, groupLength).SequenceEqual(group))
                count++;

            for (int repeats = count; repeats >= MinRepeats; repeats--)
            {
                int length = groupLength * repeats;
                results.Add(context.CreateChild(length, new PasswordMatchItem(p[..length].ToString(), PasswordMatchType.RepeatedGroup)));
            }
        }

        // Longest first

        return results.OrderByDescending(c => c.TotalMatchedLength);
    }
}

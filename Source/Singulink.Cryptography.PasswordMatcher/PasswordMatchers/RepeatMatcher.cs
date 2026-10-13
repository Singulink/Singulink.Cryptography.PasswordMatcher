namespace Singulink.Cryptography.PasswordMatchers;

/// <summary>
/// Matcher that matches another matcher a number of times in a row, within a range.
/// </summary>
/// <remarks>
/// <para>Each occurrence is matched independently, so an inner matcher that matches several alternatives (such as an <see cref="AnyMatcher"/>) can
/// match a different alternative each time. Results are produced fewest occurrences first.</para>
/// </remarks>
public sealed class RepeatMatcher : PasswordMatcher
{
    /// <summary>
    /// Gets the matcher that is repeated.
    /// </summary>
    public PasswordMatcher Matcher { get; }

    /// <summary>
    /// Gets the minimum number of occurrences that are matched.
    /// </summary>
    public int MinCount { get; }

    /// <summary>
    /// Gets the maximum number of occurrences that are matched.
    /// </summary>
    public int MaxCount { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RepeatMatcher"/> class.
    /// </summary>
    /// <param name="matcher">The matcher to repeat. Optional matchers are not permitted.</param>
    /// <param name="minCount">The minimum number of occurrences to match (at least 1).</param>
    /// <param name="maxCount">The maximum number of occurrences to match.</param>
    public RepeatMatcher(PasswordMatcher matcher, int minCount, int maxCount)
    {
        if (matcher is OptionalMatcher)
            throw new ArgumentException($"{nameof(RepeatMatcher)} cannot repeat an optional matcher.", nameof(matcher));

        if (minCount < 1)
            throw new ArgumentOutOfRangeException(nameof(minCount), "Minimum count must be at least 1.");

        if (maxCount < minCount)
            throw new ArgumentOutOfRangeException(nameof(maxCount), "Maximum count must be greater than or equal to minimum count.");

        Matcher = matcher;
        MinCount = minCount;
        MaxCount = maxCount;
    }

    /// <inheritdoc/>
    protected override IEnumerable<PasswordMatchContext> GetMatchesCore(PasswordMatchContext context)
    {
        // Occurrence by occurrence: after each one, keep only the first context reaching each distinct position, which bounds the work to
        // O(occurrences * password length) in the same way as a sequence.

        List<PasswordMatchContext> current = [context];
        var results = new List<PasswordMatchContext>();

        for (int count = 1; count <= MaxCount; count++)
        {
            var next = new List<PasswordMatchContext>();
            var seenEndPositions = new HashSet<int>();

            foreach (var c in current)
            {
                foreach (var match in Matcher.GetMatches(c))
                {
                    if (seenEndPositions.Add(match.TotalMatchedLength))
                        next.Add(match);
                }
            }

            if (next.Count is 0)
                break;

            if (count >= MinCount)
                results.AddRange(next);

            current = next;
        }

        return results;
    }
}

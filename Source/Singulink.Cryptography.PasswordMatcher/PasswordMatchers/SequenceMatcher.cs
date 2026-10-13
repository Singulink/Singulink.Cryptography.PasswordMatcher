namespace Singulink.Cryptography.PasswordMatchers;

/// <summary>
/// Matcher that matches a set of matchers one after another, in order.
/// </summary>
public sealed class SequenceMatcher : PasswordMatcher
{
    /// <summary>
    /// Gets the matchers that are matched in sequence.
    /// </summary>
    public ImmutableArray<PasswordMatcher> Matchers { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SequenceMatcher"/> class.
    /// </summary>
    /// <param name="matchers">The matchers to match in sequence. At least one must not be optional.</param>
    public SequenceMatcher(IEnumerable<PasswordMatcher> matchers)
    {
        Matchers = matchers.ToImmutableArray();

        if (Matchers.All(m => m is OptionalMatcher))
            throw new ArgumentException($"{nameof(SequenceMatcher)} cannot contain only optional matchers.", nameof(matchers));
    }

    /// <inheritdoc/>
    protected override IEnumerable<PasswordMatchContext> GetMatchesCore(PasswordMatchContext context)
    {
        // Stage by stage: after each matcher, keep only the first context reaching each distinct position. This preserves the order in which
        // the first full match would have been found by a depth-first search while bounding the work to O(stages * password length).

        List<PasswordMatchContext> current = [context];

        foreach (var matcher in Matchers)
        {
            var next = new List<PasswordMatchContext>();
            var seenEndPositions = new HashSet<int>();

            foreach (var c in current)
            {
                foreach (var match in matcher.GetMatches(c))
                {
                    if (seenEndPositions.Add(match.TotalMatchedLength))
                        next.Add(match);
                }
            }

            if (next.Count is 0)
                return [];

            current = next;
        }

        return current;
    }
}

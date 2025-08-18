namespace Singulink.Cryptography.PasswordMatchers;

/// <summary>
/// Matcher that checks for all permutations of a set of matchers in any order.
/// </summary>
public class PermutationsMatcher : PasswordMatcher
{
    public ImmutableArray<PasswordMatcher> Matchers { get; }

    public bool MustMatchAllMatchers { get; }

    public PermutationsMatcher(IEnumerable<PasswordMatcher> matchers, bool mustMatchAllMatchers)
    {
        Matchers = matchers.ToImmutableArray();
        MustMatchAllMatchers = mustMatchAllMatchers;

        if (Matchers.Distinct().Count() != Matchers.Length)
            throw new ArgumentException($"{typeof(PermutationsMatcher).Name} cannot contain multiple instances of the same matcher.", nameof(matchers));

        if (Matchers.Any(m => m is OptionalMatcher))
            throw new ArgumentException($"{typeof(PermutationsMatcher).Name} cannot directly contain optional matchers.", nameof(matchers));
    }

    protected internal override IEnumerable<PasswordMatchContext> GetMatches(PasswordMatchContext context)
    {
        return MustMatchAllMatchers
            ? GetMatchesAll(context, []).Distinct()
            : GetMatchesAny(context, []).Distinct();
    }

    private IEnumerable<PasswordMatchContext> GetMatchesAny(PasswordMatchContext context, PasswordMatcher[] usedMatchers)
    {
        foreach (var matcher in Matchers)
        {
            if (Array.IndexOf(usedMatchers, matcher) >= 0)
                continue;

            foreach (var match in matcher.GetMatches(context))
            {
                yield return match;

                PasswordMatcher[] newUsedMatchers = [..usedMatchers, matcher];

                foreach (var permutationMatches in GetMatchesAny(match, newUsedMatchers))
                    yield return permutationMatches;
            }
        }
    }

    private IEnumerable<PasswordMatchContext> GetMatchesAll(PasswordMatchContext context, PasswordMatcher[] usedMatchers)
    {
        bool isLastMatcher = Matchers.Length == usedMatchers.Length + 1;

        foreach (var matcher in Matchers)
        {
            if (Array.IndexOf(usedMatchers, matcher) >= 0)
                continue;

            foreach (var match in matcher.GetMatches(context))
            {
                if (isLastMatcher)
                    yield return match;

                PasswordMatcher[] newUsedMatchers = [.. usedMatchers, matcher];

                foreach (var permutationMatches in GetMatchesAll(match, newUsedMatchers))
                    yield return permutationMatches;
            }
        }
    }
}
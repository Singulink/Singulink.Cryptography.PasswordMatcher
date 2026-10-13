namespace Singulink.Cryptography.PasswordMatchers;

/// <summary>
/// Matcher that matches a set of matchers in any order, with each matcher used at most once.
/// </summary>
public sealed class PermutationsMatcher : PasswordMatcher
{
    /// <summary>
    /// The maximum number of matchers that can be permuted.
    /// </summary>
    public const int MaxMatchers = 31;

    /// <summary>
    /// Gets the matchers that are permuted.
    /// </summary>
    public ImmutableArray<PasswordMatcher> Matchers { get; }

    /// <summary>
    /// Gets a value indicating whether every matcher must match for the permutation to match. If <see langword="false"/>, any non-empty subset
    /// of the matchers can match.
    /// </summary>
    public bool MustMatchAllMatchers { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PermutationsMatcher"/> class.
    /// </summary>
    /// <param name="matchers">The matchers to permute. Each must be a distinct instance, and optional matchers are not permitted.</param>
    /// <param name="mustMatchAllMatchers">Whether every matcher must match for the permutation to match.</param>
    public PermutationsMatcher(IEnumerable<PasswordMatcher> matchers, bool mustMatchAllMatchers)
    {
        Matchers = matchers.ToImmutableArray();
        MustMatchAllMatchers = mustMatchAllMatchers;

        if (Matchers.Length > MaxMatchers)
            throw new ArgumentException($"{nameof(PermutationsMatcher)} supports a maximum of {MaxMatchers} matchers.", nameof(matchers));

        if (Matchers.Distinct().Count() != Matchers.Length)
            throw new ArgumentException($"{nameof(PermutationsMatcher)} cannot contain multiple instances of the same matcher.", nameof(matchers));

        if (Matchers.Any(m => m is OptionalMatcher))
            throw new ArgumentException($"{nameof(PermutationsMatcher)} cannot directly contain optional matchers.", nameof(matchers));
    }

    /// <inheritdoc/>
    protected override IEnumerable<PasswordMatchContext> GetMatchesCore(PasswordMatchContext context)
    {
        // Depth-first over (used matcher set, position) states. Revisiting a state can only produce end positions already produced.
        var visited = new HashSet<(int UsedMask, int Position)>();
        return GetMatches(context, 0, visited);
    }

    private IEnumerable<PasswordMatchContext> GetMatches(PasswordMatchContext context, int usedMask, HashSet<(int UsedMask, int Position)> visited)
    {
        if (!visited.Add((usedMask, context.TotalMatchedLength)))
            yield break;

        int allMask = (1 << Matchers.Length) - 1;

        for (int i = 0; i < Matchers.Length; i++)
        {
            int bit = 1 << i;

            if ((usedMask & bit) is not 0)
                continue;

            int newMask = usedMask | bit;

            foreach (var match in Matchers[i].GetMatches(context))
            {
                if (!MustMatchAllMatchers || newMask == allMask)
                    yield return match;

                foreach (var permutationMatch in GetMatches(match, newMask, visited))
                    yield return permutationMatch;
            }
        }
    }
}

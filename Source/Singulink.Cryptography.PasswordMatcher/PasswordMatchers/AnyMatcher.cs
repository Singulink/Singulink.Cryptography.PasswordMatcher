namespace Singulink.Cryptography.PasswordMatchers;

/// <summary>
/// Matcher that matches any one of a set of matchers.
/// </summary>
public sealed class AnyMatcher : PasswordMatcher
{
    /// <summary>
    /// Gets the matchers that are tried, in order.
    /// </summary>
    public ImmutableArray<PasswordMatcher> Matchers { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AnyMatcher"/> class.
    /// </summary>
    /// <param name="matchers">The matchers to try, in order. Optional matchers are not permitted.</param>
    public AnyMatcher(IEnumerable<PasswordMatcher> matchers)
    {
        Matchers = matchers.ToImmutableArray();

        if (Matchers.Any(m => m is OptionalMatcher))
            throw new ArgumentException($"{nameof(AnyMatcher)} cannot contain optional matchers.", nameof(matchers));
    }

    /// <inheritdoc/>
    protected override IEnumerable<PasswordMatchContext> GetMatchesCore(PasswordMatchContext context)
    {
        foreach (var matcher in Matchers)
        {
            foreach (var childContext in matcher.GetMatches(context))
                yield return childContext;
        }
    }
}

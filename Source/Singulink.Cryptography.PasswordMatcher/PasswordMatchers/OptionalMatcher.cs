namespace Singulink.Cryptography.PasswordMatchers;

/// <summary>
/// Matcher that matches another matcher or nothing at all.
/// </summary>
public sealed class OptionalMatcher : PasswordMatcher
{
    /// <summary>
    /// Gets the matcher that is made optional.
    /// </summary>
    public PasswordMatcher Matcher { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="OptionalMatcher"/> class.
    /// </summary>
    /// <param name="matcher">The matcher to make optional.</param>
    public OptionalMatcher(PasswordMatcher matcher)
    {
        Matcher = matcher;
    }

    /// <inheritdoc/>
    protected override IEnumerable<PasswordMatchContext> GetMatchesCore(PasswordMatchContext context)
    {
        yield return context;

        foreach (var childContext in Matcher.GetMatches(context))
            yield return childContext;
    }
}

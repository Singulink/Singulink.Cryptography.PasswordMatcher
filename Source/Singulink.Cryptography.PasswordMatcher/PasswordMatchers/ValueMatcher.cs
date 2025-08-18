namespace Singulink.Cryptography.PasswordMatchers;

public abstract class ValueMatcher : PasswordMatcher
{
    private static readonly ImmutableArray<char> Separators = [' ', '.'];

    public bool MatchTrailingSeparator { get; }

    /// <summary>
    /// Gets a value indicating whether repeated values are matched.
    /// </summary>
    public bool MatchRepeats { get; }

    protected ValueMatcher(bool matchRepeats, bool matchTrailingSeparator)
    {
        MatchRepeats = matchRepeats;
        MatchTrailingSeparator = matchTrailingSeparator;
    }

    protected internal sealed override IEnumerable<PasswordMatchContext> GetMatches(PasswordMatchContext context)
    {
        foreach (var childContext in GetValueMatches(context))
        {
            yield return childContext;

            RepeatValueMatcher repeatValueMatcher = null;

            if (MatchRepeats)
            {
                int repeatValueLength = childContext.TotalMatchedLength - context.TotalMatchedLength;

                if (repeatValueLength > 0)
                {
                    string repeatValue = context.CheckedPassword.Substring(context.TotalMatchedLength, repeatValueLength);
                    repeatValueMatcher = new(this, repeatValue, childContext.LastMatchItem?.MatchedText, MatchTrailingSeparator);

                    foreach (var repeatContext in repeatValueMatcher.GetMatches(childContext))
                        yield return repeatContext;
                }
            }

            if (MatchTrailingSeparator && childContext.RemainingChars is [char s, ..] && Separators.Contains(s))
            {
                var childContextWithSeparator = childContext.CreateChild(this, 1, null);
                yield return childContextWithSeparator;

                if (repeatValueMatcher is not null)
                {
                    foreach (var repeatContext in repeatValueMatcher.GetValueMatches(childContextWithSeparator))
                        yield return repeatContext;
                }
            }
        }
    }

    protected abstract IEnumerable<PasswordMatchContext> GetValueMatches(PasswordMatchContext context);
}

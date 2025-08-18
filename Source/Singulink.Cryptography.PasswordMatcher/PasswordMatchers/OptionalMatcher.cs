namespace Singulink.Cryptography.PasswordMatchers;

public class OptionalMatcher : PasswordMatcher
{
    public PasswordMatcher Matcher { get; }

    public OptionalMatcher(PasswordMatcher matcher)
    {
        Matcher = matcher;
    }

    protected internal override IEnumerable<PasswordMatchContext> GetMatches(PasswordMatchContext context)
    {
        yield return context;

        foreach (var childContext in Matcher.GetMatches(context))
            yield return childContext;
    }
}

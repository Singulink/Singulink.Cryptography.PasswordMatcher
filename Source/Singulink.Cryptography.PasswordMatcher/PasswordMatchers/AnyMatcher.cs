namespace Singulink.Cryptography.PasswordMatchers;

public class AnyMatcher : PasswordMatcher
{
    public ImmutableArray<PasswordMatcher> Matchers { get; }

    public AnyMatcher(IEnumerable<PasswordMatcher> matchers)
    {
        Matchers = matchers.ToImmutableArray();

        if (Matchers.Any(m => m is OptionalMatcher))
            throw new ArgumentException($"{typeof(AnyMatcher).Name} cannot contain optional matchers.", nameof(matchers));
    }

    protected internal override IEnumerable<PasswordMatchContext> GetMatches(PasswordMatchContext context)
    {
        foreach (var matcher in Matchers)
        {
            foreach (var childContext in matcher.GetMatches(context))
            {
                yield return childContext;
            }
        }
    }
}

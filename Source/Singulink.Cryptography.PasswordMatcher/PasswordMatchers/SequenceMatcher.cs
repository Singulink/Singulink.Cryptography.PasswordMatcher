namespace Singulink.Cryptography.PasswordMatchers;

public class SequenceMatcher : PasswordMatcher
{
    public ImmutableArray<PasswordMatcher> Matchers { get; }

    public SequenceMatcher(IEnumerable<PasswordMatcher> matchers)
    {
        Matchers = matchers.ToImmutableArray();

        if (Matchers.All(m => m is OptionalMatcher))
            throw new ArgumentException($"{typeof(SequenceMatcher).Name} cannot contain only optional matchers.", nameof(matchers));
    }

    protected internal override IEnumerable<PasswordMatchContext> GetMatches(PasswordMatchContext context) => GetMatches(context, 0);

    private IEnumerable<PasswordMatchContext> GetMatches(PasswordMatchContext context, int index)
    {
        if (index >= Matchers.Length)
        {
            yield return context;
            yield break;
        }

        var matcher = Matchers[index];

        foreach (var match in matcher.GetMatches(context))
        {
            foreach (var nextMatch in GetMatches(match, index + 1))
                yield return nextMatch;
        }
    }
}

namespace Singulink.Cryptography;

public class PasswordMatchEvaluator : IPasswordMatchEvaluator
{
    public static PasswordMatchEvaluator Default { get; } = new PasswordMatchEvaluator(PasswordMatchersProvider.Default);

    private readonly IPasswordMatchersProvider _matchersProvider;

    public PasswordMatchEvaluator(IPasswordMatchersProvider matchersProvider)
    {
        _matchersProvider = matchersProvider;
    }

    public virtual PasswordMatchResult MatchPassword(string password, IEnumerable<ContextualSubject>? subjects)
    {
        return GetResult(password, _matchersProvider.GetMatchers(subjects));
    }

    public static PasswordMatchResult GetResult(string password, IEnumerable<PasswordMatcher> matchers)
    {
        var match = PasswordMatcher.GetFirstMatch(password, matchers);

        return new PasswordMatchResult {
            Matched = match is not null,
            MatchedValues = match?.Items.Select(i => i.MatchedText).ToList() ?? [],
        };
    }
}

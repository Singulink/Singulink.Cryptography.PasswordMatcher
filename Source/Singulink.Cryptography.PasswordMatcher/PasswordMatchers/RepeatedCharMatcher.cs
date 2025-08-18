namespace Singulink.Cryptography.PasswordMatchers;

public class RepeatedCharMatcher : ValueMatcher
{
    public int MinCount { get; }

    public int MaxCount { get; }

    public Func<char, bool>? CharFilter { get; set; }

    public RepeatedCharMatcher(int minCount, int maxCount, Func<char, bool>? charFilter, bool matchTrailingSeparator)
        : base(matchRepeats: false, matchTrailingSeparator)
    {
        if (minCount < 1)
            throw new ArgumentOutOfRangeException(nameof(minCount), "Minimum count must be at least 1.");

        if (maxCount < minCount)
            throw new ArgumentOutOfRangeException(nameof(maxCount), "Maximum count must be greater than or equal to minimum count.");

        MinCount = minCount;
        MaxCount = maxCount;
        CharFilter = charFilter;
    }

    protected override IEnumerable<PasswordMatchContext> GetValueMatches(PasswordMatchContext context) => GetValueMatchesImpl(context).Reverse();

    private IEnumerable<PasswordMatchContext> GetValueMatchesImpl(PasswordMatchContext context)
    {
        if (context.RemainingChars.Length < MinCount)
            yield break;

        char c = context.RemainingChars[0];

        if (CharFilter is not null && !CharFilter(c))
            yield break;

        int count = 1;

        if (MinCount is 1)
            yield return context.CreateChild(this, 1, new PasswordMatchItem(c.ToString(), PasswordMatchType.RepeatedChar));

        for (int i = 1; i < context.RemainingChars.Length && count < MaxCount; i++)
        {
            if (context.RemainingChars[i] == c)
            {
                count++;

                if (count >= MinCount)
                {
                    var matchItem = new PasswordMatchItem(context.RemainingChars[..(i + 1)].ToString(), PasswordMatchType.RepeatedChar);
                    yield return context.CreateChild(this, i + 1, matchItem);
                }
            }
            else
            {
                yield break;
            }
        }
    }
}
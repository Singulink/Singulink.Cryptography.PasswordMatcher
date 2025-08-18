using Singulink.Cryptography.PasswordMatchers;

namespace Singulink.Cryptography;

public abstract class PasswordMatcher
{
    protected internal abstract IEnumerable<PasswordMatchContext> GetMatches(PasswordMatchContext context);

    public static PasswordMatch? GetFirstMatch(string password, IEnumerable<PasswordMatcher> matchers)
    {
        return GetAllMatches(password, matchers).FirstOrDefault();
    }

    public static IEnumerable<PasswordMatch> GetAllMatches(string password, IEnumerable<PasswordMatcher> matchers)
    {
        var context = new PasswordMatchContext(password);
        return GetAllMatches(matchers.SelectMany(m => m.GetMatches(context)));
    }

    private static IEnumerable<PasswordMatch> GetAllMatches(IEnumerable<PasswordMatchContext> contexts)
    {
        return contexts
            .Where(c => c.TotalMatchedLength == c.OriginalPassword.Length)
            .Select(ContextToMatchResult);

        static PasswordMatch ContextToMatchResult(PasswordMatchContext context)
        {
            var items = new List<PasswordMatchItem>();

            for (var c = context; c.ParentContext is not null; c = c.ParentContext)
            {
                if (c.LastMatchItem is not null)
                    items.Add(c.LastMatchItem);
            }

            items.Reverse();

            return new PasswordMatch { Items = items };
        }
    }

    public static PasswordMatcher Any(IEnumerable<PasswordMatcher> matchers)
    {
        return new AnyMatcher(matchers);
    }

    public static PasswordMatcher FixedDigits(int numDigits, PasswordMatchType matchType, bool matchRepeats = true, bool matchTrailingSeparator = true)
    {
        return new FixedDigitsMatcher(numDigits, minValue: 0, maxValue: (int)Math.Pow(10, numDigits) - 1, matchType, matchRepeats, matchTrailingSeparator);
    }

    public static PasswordMatcher FixedDigits(int numDigits, int minValue, int maxValue, PasswordMatchType matchType, bool matchRepeats = true, bool matchTrailingSeparator = true)
    {
        return new FixedDigitsMatcher(numDigits, minValue, maxValue, matchType, matchRepeats, matchTrailingSeparator);
    }

    public static PasswordMatcher KeyboardSequence(KeyboardSequenceTypes sequenceTypes, bool matchTrailingSeparator = true)
    {
        return new KeyboardSequenceMatcher(sequenceTypes, matchTrailingSeparator);
    }

    public static PasswordMatcher Optional(PasswordMatcher matcher)
    {
        return new OptionalMatcher(matcher);
    }

    public static PasswordMatcher Permutations(IEnumerable<PasswordMatcher> matchers, bool mustMatchAllMatchers = false)
    {
        return new PermutationsMatcher(matchers, mustMatchAllMatchers);
    }

    public static PasswordMatcher RepeatedChars(Func<char, bool>? charFilter, bool matchTrailingSeparator = true)
    {
        return new RepeatedCharMatcher(1, int.MaxValue, charFilter, matchTrailingSeparator);
    }

    public static PasswordMatcher RepeatedChars(int minCount, int maxCount, Func<char, bool>? charFilter, bool matchTrailingSeparator = true)
    {
        return new RepeatedCharMatcher(minCount, maxCount, charFilter, matchTrailingSeparator);
    }

    public static PasswordMatcher Text(string text, PasswordMatchType matchType = PasswordMatchType.Common, bool checkSubstitutions = true, bool matchRepeats = true, bool matchTrailingSeparator = true)
    {
        return new TextMatcher(text, matchType, checkSubstitutions, matchRepeats, matchTrailingSeparator);
    }

    public static PasswordMatcher Sequence(IEnumerable<PasswordMatcher> matchers)
    {
        return new SequenceMatcher(matchers);
    }
}

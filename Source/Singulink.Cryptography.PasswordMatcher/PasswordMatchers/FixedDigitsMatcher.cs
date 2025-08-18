using Singulink.Enums;

namespace Singulink.Cryptography.PasswordMatchers;

public class FixedDigitsMatcher : ValueMatcher
{
    public int NumDigits { get; }

    public int MinValue { get; }

    public int MaxValue { get; }

    public PasswordMatchType MatchType { get; }

    public FixedDigitsMatcher(int numDigits, int minValue, int maxValue, PasswordMatchType matchType, bool matchRepeats, bool matchTrailingSeparator)
        : base(matchRepeats, matchTrailingSeparator)
    {
        if (numDigits < 1 || numDigits > 9)
            throw new ArgumentOutOfRangeException(nameof(numDigits), "Number of digits must be between 1 and 9.");

        if (minValue < 0 || maxValue < minValue || maxValue > Math.Pow(10, numDigits) - 1)
            throw new ArgumentOutOfRangeException(nameof(minValue), "Invalid range for digit matcher.");

        matchType.ThrowIfNotDefined(nameof(matchType));

        NumDigits = numDigits;
        MinValue = minValue;
        MaxValue = maxValue;
        MatchType = matchType;
    }

    protected override IEnumerable<PasswordMatchContext> GetValueMatches(PasswordMatchContext context)
    {
        var p = context.RemainingChars;

        if (p.Length < NumDigits)
            return [];

#if NET
        bool parseResult = int.TryParse(p[..NumDigits], out int value);
#else
        bool parseResult = int.TryParse(p[..NumDigits].ToString(), out int value);
#endif

        if (!parseResult || value < MinValue || value > MaxValue)
            return [];

        return [context.CreateChild(this, NumDigits, new PasswordMatchItem(p[..NumDigits].ToString(), MatchType))];
    }
}

using Singulink.Enums;

namespace Singulink.Cryptography.PasswordMatchers;

/// <summary>
/// Matcher that matches a fixed number of digits whose value falls within a range, such as a year.
/// </summary>
public sealed class FixedDigitsMatcher : ValueMatcher
{
    /// <summary>
    /// Gets the number of digits that are matched.
    /// </summary>
    public int NumDigits { get; }

    /// <summary>
    /// Gets the minimum value of the digits (inclusive).
    /// </summary>
    public int MinValue { get; }

    /// <summary>
    /// Gets the maximum value of the digits (inclusive).
    /// </summary>
    public int MaxValue { get; }

    /// <summary>
    /// Gets the type of match that is reported.
    /// </summary>
    public PasswordMatchType MatchType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FixedDigitsMatcher"/> class.
    /// </summary>
    /// <param name="numDigits">The number of digits to match (between 1 and 9).</param>
    /// <param name="minValue">The minimum value of the digits (inclusive).</param>
    /// <param name="maxValue">The maximum value of the digits (inclusive).</param>
    /// <param name="matchType">The type of match to report.</param>
    /// <param name="matchRepeats">Whether consecutive repeats of the matched digits are also matched.</param>
    /// <param name="matchTrailingSeparator">Whether a single separator character after the digits is also matched.</param>
    public FixedDigitsMatcher(int numDigits, int minValue, int maxValue, PasswordMatchType matchType, bool matchRepeats, bool matchTrailingSeparator)
        : base(matchRepeats, matchTrailingSeparator)
    {
        if (numDigits < 1 || numDigits > 9)
            throw new ArgumentOutOfRangeException(nameof(numDigits), "Number of digits must be between 1 and 9.");

        if (minValue < 0 || maxValue < minValue || maxValue > Math.Pow(10, numDigits) - 1)
            throw new ArgumentOutOfRangeException(nameof(minValue), "Invalid value range for the number of digits.");

        matchType.ThrowIfNotDefined(nameof(matchType));

        NumDigits = numDigits;
        MinValue = minValue;
        MaxValue = maxValue;
        MatchType = matchType;
    }

    /// <inheritdoc/>
    protected override IEnumerable<PasswordMatchContext> GetValueMatches(PasswordMatchContext context)
    {
        var p = context.RemainingChars;

        if (p.Length < NumDigits)
            return [];

        int value = 0;

        for (int i = 0; i < NumDigits; i++)
        {
            char c = p[i];

            if (c is < '0' or > '9')
                return [];

            value = (value * 10) + (c - '0');
        }

        if (value < MinValue || value > MaxValue)
            return [];

        return [context.CreateChild(NumDigits, new PasswordMatchItem(p[..NumDigits].ToString(), MatchType))];
    }
}

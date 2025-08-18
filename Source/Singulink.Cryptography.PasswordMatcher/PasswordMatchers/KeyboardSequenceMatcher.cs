using Singulink.Enums;

namespace Singulink.Cryptography.PasswordMatchers;

public class KeyboardSequenceMatcher : ValueMatcher
{
    private static readonly ImmutableArray<string> Rows = GetRows();

    public KeyboardSequenceTypes SequenceTypes { get; }

    public KeyboardSequenceMatcher(KeyboardSequenceTypes sequenceTypes, bool matchTrailingSeparator)
        : base(true, matchTrailingSeparator)
    {
        if (!sequenceTypes.IsValid())
            throw new ArgumentOutOfRangeException(nameof(sequenceTypes), "Invalid sequence types specified.");

        if (sequenceTypes == KeyboardSequenceTypes.None)
            throw new ArgumentException("At least one sequence type must be specified.", nameof(sequenceTypes));

        SequenceTypes = sequenceTypes;
    }

    protected override IEnumerable<PasswordMatchContext> GetValueMatches(PasswordMatchContext context) => GetValueMatchesImpl(context).Distinct().Reverse();

    private IEnumerable<PasswordMatchContext> GetValueMatchesImpl(PasswordMatchContext context)
    {
        if ((SequenceTypes & KeyboardSequenceTypes.Row) is not 0)
        {
            foreach (string row in Rows)
            {
                // Check if remaining characters start with 3 consecutive letters anywhere in the row

                for (int i = 0; i <= row.Length - 3; i++)
                {
                    if (context.RemainingChars.StartsWith(row.AsSpan(i, 3), StringComparison.Ordinal))
                    {
                        var matchItem = new PasswordMatchItem(row.Substring(i, 3).ToUpperInvariant(), PasswordMatchType.KeyboardSequence);
                        var matchContext = context.CreateChild(this, 3, matchItem);

                        yield return matchContext;

                        // Check for each additional character match

                        int currentChildMatchingLength = 4;

                        for (int j = i + 3; j < row.Length && matchContext.RemainingChars.Length > 0; j++)
                        {
                            if (matchContext.RemainingChars[0] == row[j])
                            {
                                matchItem = new PasswordMatchItem(matchItem.MatchedText + char.ToUpperInvariant(row[j]), PasswordMatchType.KeyboardSequence);
                                matchContext = context.CreateChild(this, currentChildMatchingLength++, matchItem);
                                yield return matchContext;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }

        // TODO: Add additional keyboard sequence types.
    }

    private static ImmutableArray<string> GetRows()
    {
        var forward = KeyboardData.EnglishUSRows.Select(r => r.ToLowerInvariant()).ToImmutableArray();
        var backward = forward.Select(r => new string(r.Reverse().ToArray())).ToImmutableArray();
        var forwardShifted = forward.Select(r => new string(r.Select(GetShifted).ToArray())).ToImmutableArray();
        var backwardShifted = backward.Select(r => new string(r.Select(GetShifted).ToArray())).ToImmutableArray();

        return [..forward, ..backward, ..forwardShifted, ..backwardShifted];
    }

    private static char GetShifted(char c)
    {
        string ks = KeyboardData.EnglishUSKeysToShiftedKeys.FirstOrDefault(ks => ks[0] == c);
        return ks is not null ? ks[1] : c;
    }
}

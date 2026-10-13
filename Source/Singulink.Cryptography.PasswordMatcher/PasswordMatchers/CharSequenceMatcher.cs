using Singulink.Enums;

namespace Singulink.Cryptography.PasswordMatchers;

/// <summary>
/// Matcher that matches sequences of characters that are adjacent on a keyboard or keypad or consecutive in the alphabet, such as "qwerty",
/// "zaq1", "!@#$", "1q2w3e4r", "7410" or "abcdef".
/// </summary>
/// <remarks>
/// <para>Sequences are matched in both directions along each line of characters, and keyboard sequences are also matched with the shift key held.
/// A sequence must be at least <see cref="MinSequenceLength"/> characters long. The longest sequence is matched first.</para>
/// </remarks>
public sealed class CharSequenceMatcher : ValueMatcher
{
    /// <summary>
    /// The minimum number of characters in a sequence.
    /// </summary>
    public const int MinSequenceLength = 3;

    /// <summary>
    /// The letters of the alphabet, in order.
    /// </summary>
    public const string Alphabet = "abcdefghijklmnopqrstuvwxyz";

    private static readonly ImmutableArray<string> KeyboardRowLines = GetLines(KeyboardData.EnglishUSRows, includeShifted: true);

    private static readonly ImmutableArray<string> KeyboardDiagonalLines = GetLines([..KeyboardData.EnglishUSForwardDiagonals, ..KeyboardData.EnglishUSBackwardDiagonals], includeShifted: true);

    private static readonly ImmutableArray<string> KeyboardZigzagLines = GetLines(KeyboardData.EnglishUSZigzags, includeShifted: true);

    private static readonly ImmutableArray<string> NumericKeypadLines = GetLines(KeyboardData.NumericKeypadLines, includeShifted: false);

    private static readonly ImmutableArray<string> AlphabetLines = GetLines([Alphabet], includeShifted: false);

    /// <summary>
    /// Gets the types of sequences that are matched.
    /// </summary>
    public CharSequenceTypes SequenceTypes { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CharSequenceMatcher"/> class.
    /// </summary>
    /// <param name="sequenceTypes">The types of sequences to match.</param>
    /// <param name="matchTrailingSeparator">Whether a single separator character after the sequence is also matched.</param>
    public CharSequenceMatcher(CharSequenceTypes sequenceTypes, bool matchTrailingSeparator)
        : base(true, matchTrailingSeparator)
    {
        if (!sequenceTypes.IsValid())
            throw new ArgumentOutOfRangeException(nameof(sequenceTypes), "Invalid sequence types specified.");

        if (sequenceTypes == CharSequenceTypes.None)
            throw new ArgumentException("At least one sequence type must be specified.", nameof(sequenceTypes));

        SequenceTypes = sequenceTypes;
    }

    /// <inheritdoc/>
    protected override IEnumerable<PasswordMatchContext> GetValueMatches(PasswordMatchContext context)
    {
        var results = new List<PasswordMatchContext>();

        AddLineMatches(CharSequenceTypes.KeyboardRow, KeyboardRowLines);
        AddLineMatches(CharSequenceTypes.KeyboardDiagonal, KeyboardDiagonalLines);
        AddLineMatches(CharSequenceTypes.KeyboardZigzag, KeyboardZigzagLines);
        AddLineMatches(CharSequenceTypes.NumericKeypad, NumericKeypadLines);
        AddLineMatches(CharSequenceTypes.Alphabet, AlphabetLines);

        // Longest first (stable, so earlier sequence types win ties)

        return results.OrderByDescending(c => c.TotalMatchedLength);

        void AddLineMatches(CharSequenceTypes type, ImmutableArray<string> lines)
        {
            if ((SequenceTypes & type) is 0)
                return;

            foreach (string line in lines)
            {
                for (int i = 0; i <= line.Length - MinSequenceLength; i++)
                {
                    // Check if the remaining characters start with a sequence from the line, and find how far the sequence extends.

                    if (!context.RemainingChars.StartsWith(line.AsSpan(i, MinSequenceLength), StringComparison.Ordinal))
                        continue;

                    int length = MinSequenceLength;

                    while (i + length < line.Length && length < context.RemainingChars.Length && context.RemainingChars[length] == line[i + length])
                        length++;

                    for (int matchLength = MinSequenceLength; matchLength <= length; matchLength++)
                    {
                        var matchItem = new PasswordMatchItem(line.Substring(i, matchLength).ToUpperInvariant(), PasswordMatchType.CharSequence);
                        results.Add(context.CreateChild(matchLength, matchItem));
                    }
                }
            }
        }
    }

    private static ImmutableArray<string> GetLines(IEnumerable<string> lines, bool includeShifted)
    {
        var forward = lines.Select(l => l.ToLowerInvariant()).ToList();
        var backward = forward.Select(l => new string(l.Reverse().ToArray())).ToList();
        IEnumerable<string> all = [..forward, ..backward];

        if (includeShifted)
        {
            var forwardShifted = forward.Select(l => new string(l.Select(GetShifted).ToArray()));
            var backwardShifted = backward.Select(l => new string(l.Select(GetShifted).ToArray()));
            all = [..all, ..forwardShifted, ..backwardShifted];
        }

        return all.Distinct().ToImmutableArray();
    }

    private static char GetShifted(char c)
    {
        string? ks = KeyboardData.EnglishUSKeysToShiftedKeys.FirstOrDefault(ks => ks[0] == c);
        return ks is not null ? ks[1] : c;
    }
}

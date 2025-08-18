using Singulink.Enums;

namespace Singulink.Cryptography.PasswordMatchers;

public class TextMatcher : ValueMatcher
{
    private static readonly FrozenDictionary<char, ImmutableArray<char>> CharacterSubstitutions = new Dictionary<char, ImmutableArray<char>>() {
                { 'a', ['4', '@', 'Д'] },
                { 'b', ['8', 'ß'] },
                { 'c', ['(', '<', '{', '[', '¢'] },
                { 'e', ['3', '€', '£'] },
                { 'f', ['ƒ'] },
                { 'g', ['6', '9'] },
                { 'i', ['1', '!', '|'] },
                { 'l', ['1', '|', '7'] },
                { 'n', ['И', 'ท'] },
                { 'o', ['0', 'Ø'] },
                { 'r', ['®', 'Я'] },
                { 's', ['5', '$'] },
                { 't', ['7', '+'] },
                { 'u', ['µ', 'บ'] },
                { 'w', ['พ', '₩', 'ω'] },
                { 'x', ['×'] },
                { 'y', ['¥'] },
                { 'z', ['2', '%'] },
            }.ToFrozenDictionary();

    public new string Text { get; }

    public bool CheckSubstitutions { get; }

    public PasswordMatchType MatchType { get; }

    public TextMatcher(string text, PasswordMatchType matchType, bool checkSubstitutions, bool matchRepeats, bool matchTrailingSeparator)
        : base(matchRepeats, matchTrailingSeparator)
    {
        text = text.Trim().ToLowerInvariant();

        if (text.Length is 0)
            throw new ArgumentException("Segment cannot be empty.", nameof(text));

        matchType.ThrowIfNotDefined(nameof(matchType));

        Text = text;
        CheckSubstitutions = checkSubstitutions;
        MatchType = matchType;
    }

    protected override IEnumerable<PasswordMatchContext> GetValueMatches(PasswordMatchContext context)
    {
        var p = context.RemainingChars;

        if (p.Length < Text.Length)
            return [];

        for (int i = 0; i < Text.Length; i++)
        {
            if (p[i] != Text[i])
            {
                if (!CheckSubstitutions || !CharacterSubstitutions.TryGetValue(Text[i], out var substitutions) || !substitutions.Contains(p[i]))
                    return [];
            }
        }

        return [context.CreateChild(this, Text.Length, new PasswordMatchItem(Text, MatchType))];
    }
}

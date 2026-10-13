using Singulink.Enums;

namespace Singulink.Cryptography.PasswordMatchers;

/// <summary>
/// Matcher that matches a word or phrase, ignoring case and optionally allowing common character substitutions (such as "@" for "a" or "0" for
/// "o").
/// </summary>
public sealed class TextMatcher : ValueMatcher
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

    /// <summary>
    /// Gets the normalized (trimmed and lower-cased) text that is matched.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Gets a value indicating whether common character substitutions are matched in place of the original characters.
    /// </summary>
    public bool CheckSubstitutions { get; }

    /// <summary>
    /// Gets the type of match that is reported.
    /// </summary>
    public PasswordMatchType MatchType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TextMatcher"/> class.
    /// </summary>
    /// <param name="text">The text to match.</param>
    /// <param name="matchType">The type of match to report.</param>
    /// <param name="checkSubstitutions">Whether common character substitutions are matched in place of the original characters.</param>
    /// <param name="matchRepeats">Whether consecutive repeats of the matched text are also matched.</param>
    /// <param name="matchTrailingSeparator">Whether a single separator character after the text is also matched.</param>
    public TextMatcher(string text, PasswordMatchType matchType, bool checkSubstitutions, bool matchRepeats, bool matchTrailingSeparator)
        : base(matchRepeats, matchTrailingSeparator)
    {
        text = text.Trim().ToLowerInvariant();

        if (text.Length is 0)
            throw new ArgumentException("Text cannot be empty.", nameof(text));

        matchType.ThrowIfNotDefined(nameof(matchType));

        Value = text;
        CheckSubstitutions = checkSubstitutions;
        MatchType = matchType;
    }

    /// <inheritdoc/>
    protected override IEnumerable<PasswordMatchContext> GetValueMatches(PasswordMatchContext context)
    {
        var p = context.RemainingChars;

        if (p.Length < Value.Length)
            return [];

        for (int i = 0; i < Value.Length; i++)
        {
            if (p[i] != Value[i])
            {
                if (!CheckSubstitutions || !CharacterSubstitutions.TryGetValue(Value[i], out var substitutions) || !substitutions.Contains(p[i]))
                    return [];
            }
        }

        return [context.CreateChild(Value.Length, new PasswordMatchItem(Value, MatchType))];
    }
}

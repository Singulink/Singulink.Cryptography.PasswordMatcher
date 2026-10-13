using static Singulink.Cryptography.PasswordMatcher;

namespace Singulink.Cryptography.PasswordMatchers;

/// <summary>
/// Provides predefined matchers for the most common words, phrases and values used in passwords.
/// </summary>
public static class CommonMatchers
{
    /// <summary>
    /// Gets a matcher that matches common years (1900 to 10 years from now).
    /// </summary>
    public static PasswordMatcher YearMatcher { get; } = FixedDigits(4, 1900, DateTime.Now.Year + 10, PasswordMatchType.Common);

    /// <summary>
    /// Gets a matcher that matches common adjectives, such as "best" or "cool".
    /// </summary>
    public static PasswordMatcher AdjectiveMatcher { get; } = Any([
        Word("best"),
        Word("worst"),
        Word("good"),
        Word("bad"),
        Word("cool"),
        Word("great"),
        Word("awesome"),
        Word("amazing"),
        Word("lovely"),
        Word("sexy"),
        Word("hot"),
        Word("trusted"),
    ]);

    /// <summary>
    /// Gets a matcher that matches words connecting an adjective to a following subject, such as the "as" in "cool as a ninja".
    /// </summary>
    public static PasswordMatcher AdjectiveToSubjectConjunctionMatcher { get; } = Any([
        Word("like"),
        Word("as"),
    ]);

    /// <summary>
    /// Gets a matcher that matches determiners between a copular verb and an adjective, such as the "the" in "is the best".
    /// </summary>
    public static PasswordMatcher SubjectToAdjectiveDeterminerMatcher { get; } = Any([
        Word("the"),
        Word("a"),
        Word("an"),
    ]);

    /// <summary>
    /// Gets a matcher that matches modifiers between a copular verb and an adjective, such as the "so" in "is so cool".
    /// </summary>
    public static PasswordMatcher SubjectToAdjectiveModifierMatcher { get; } = Word("so");

    /// <summary>
    /// Gets a matcher that matches copular verbs connecting a subject to an adjective, along with any modifier and determiner, such as the "is so
    /// the" in "password is so the best".
    /// </summary>
    public static PasswordMatcher SubjectToAdjectiveCopularVerbMatcher { get; } = Any([
        Sequence([Word("is"), Optional(SubjectToAdjectiveModifierMatcher), Optional(SubjectToAdjectiveDeterminerMatcher)]),
        Sequence([Word("are"), Optional(SubjectToAdjectiveModifierMatcher), Optional(SubjectToAdjectiveDeterminerMatcher)]),
        Sequence([Word("am"), Optional(SubjectToAdjectiveModifierMatcher), Optional(SubjectToAdjectiveDeterminerMatcher)]),
    ]);

    /// <summary>
    /// Gets a matcher that matches copular verbs connecting two subjects, such as the "am" in "i am admin".
    /// </summary>
    public static PasswordMatcher SubjectToSubjectCopularVerbMatcher { get; } = Any([
        Word("is"),
        Word("am"),
        Word("are"),
    ]);

    /// <summary>
    /// Gets a matcher that matches conjunctions and prepositions connecting two subjects, such as the "and" in "me and you".
    /// </summary>
    public static PasswordMatcher SubjectConjunctionMatcher { get; } = Any([
        Word("and"),
        Word("or"),
        Word("to"),
        Word("in"),
        Word("into"),
        Sequence([Word("in"), Word("to")]),
    ]);

    /// <summary>
    /// Gets a matcher that matches determiners preceding a subject, such as the "my" in "my password".
    /// </summary>
    public static PasswordMatcher SubjectDeterminerMatcher { get; } = Any([
        Word("the"),
        Word("a"),
        Word("an"),
        Word("this"),
        Word("my"),
        Word("your"),
        Word("their"),
        Word("trusted"),
        Word("into"),
        Word("let"),
    ]);

    /// <summary>
    /// Gets a matcher that matches the most common password words, such as "password", "admin" and "qwerty"-style favorites like "dragon" and
    /// "monkey".
    /// </summary>
    public static PasswordMatcher GeneralSubjectMatcher { get; } = Any([
        Word("i"),
        Word("me"),
        Word("you"),
        Word("this"),

        Word("password"),
        Word("pass"),
        Word("login"),
        Word("secret"),
        Sequence([Word("let"), Word("me"), Any([Word("into"), Word("in")])]),

        Word("welcome"),
        Word("hello"),
        Word("hi"),

        Word("master"),
        Word("admin"),
        Word("user"),
        Word("nobody"),
        Word("no1"),

        Word("football"),
        Word("baseball"),
        Word("hockey"),
        Word("soccer"),

        Word("noob"),
        Word("google"),
        Word("monkey"),
        Word("dragon"),
        Word("ninja"),
        Word("batman"),
        Word("hottie"),
        Word("shadow"),
        Word("sunshine"),
        Word("princess"),
        Word("freedom"),
        Word("jesus"),
        Word("god"),
        Word("whatever"),
        Word("love"),
        Word("sex"),
        Word("fuck"),
        Word("shit"),
        Word("starwars"),
        Sequence([Word("star"), Word("wars")]),

        Text("69", checkSubstitutions: false),
        Text("420", checkSubstitutions: false),
    ]);

    /// <summary>
    /// Gets a matcher that matches common verbs, such as the "love" in "i love you".
    /// </summary>
    public static PasswordMatcher VerbMatcher { get; } = Any([
        Word("love"),
        Word("like"),
        Word("hate"),
        Word("am"),
        Word("want"),
        Word("trust"),

        Word("login"),
        Word("log"),

        Word("let"),
    ]);

    /// <summary>
    /// Gets a matcher that matches words following a verb, such as the "to" in "want to".
    /// </summary>
    public static PasswordMatcher VerbSuffixMatcher { get; } = Word("to");

    /// <summary>
    /// Gets a matcher that matches any word from any of the word lists: subjects, adjectives, verbs, determiners, conjunctions and the other
    /// connecting words.
    /// </summary>
    public static PasswordMatcher AllWordsMatcher { get; } = Any([
        GeneralSubjectMatcher,
        AdjectiveMatcher,
        VerbMatcher,
        SubjectDeterminerMatcher,
        SubjectConjunctionMatcher,
        SubjectToSubjectCopularVerbMatcher,
        AdjectiveToSubjectConjunctionMatcher,
        VerbSuffixMatcher,
        SubjectToAdjectiveModifierMatcher,
    ]);

    /// <summary>
    /// Gets a matcher that matches every type of character sequence: keyboard rows, diagonals and zigzags, keypad lines and the alphabet.
    /// </summary>
    public static PasswordMatcher AllCharSequencesMatcher { get; } = CharSequence(CharSequenceTypes.All);

    /// <summary>
    /// Gets a matcher that matches a run of two or more of the same digit, such as "111".
    /// </summary>
    public static PasswordMatcher DigitRunMatcher { get; } = RepeatedChars(2, int.MaxValue, char.IsDigit);

    /// <summary>
    /// Gets a matcher that matches a run of two or more of the same symbol or punctuation character, such as "!!!".
    /// </summary>
    public static PasswordMatcher SymbolRunMatcher { get; } = RepeatedChars(2, int.MaxValue, IsSymbol);

    /// <summary>
    /// Gets a matcher that matches a single digit or symbol, such as the "!" joining the words in "ninja!admin" or the "1" in "mike1acme".
    /// </summary>
    public static PasswordMatcher JoinerMatcher { get; } = AnyChars(1, 1, c => char.IsDigit(c) || IsSymbol(c));

    /// <summary>
    /// Gets a matcher that matches a group of two or three digits or symbols repeated two or more times, such as "121212" or "1!1!".
    /// </summary>
    public static PasswordMatcher DigitOrSymbolGroupMatcher { get; } = RepeatedGroups(2, 3, 2, c => char.IsDigit(c) || IsSymbol(c));

    /// <summary>
    /// Gets a matcher that matches a single structured decoration: a year, a character sequence, a run of a repeated digit or symbol, or a
    /// repeated group of digits or symbols.
    /// </summary>
    public static PasswordMatcher StructuredDecorationMatcher { get; } = Any([
        YearMatcher,
        AllCharSequencesMatcher,
        DigitRunMatcher,
        SymbolRunMatcher,
        DigitOrSymbolGroupMatcher,
    ]);

    /// <summary>
    /// Gets a matcher that matches one to six structured decorations in a row, in any order.
    /// </summary>
    public static PasswordMatcher DecorationRunMatcher { get; } = Repeat(StructuredDecorationMatcher, 1, 6);

    /// <summary>
    /// Gets a matcher that matches one to four arbitrary digits or symbols (other than separators), such as the "#1" in "password#1" or the
    /// "4792" in "password4792".
    /// </summary>
    public static PasswordMatcher ShortDecorationMatcher { get; } = AnyChars(1, 4, c => !char.IsLetter(c) && !char.IsWhiteSpace(c) && !ValueMatcher.Separators.Contains(c));

    /// <summary>
    /// Gets a matcher that matches the decorations that commonly precede or follow the words of a password: any number of structured decorations
    /// (years, character sequences, runs and repeated groups) in any order, with at most one short run of arbitrary digits or symbols among them.
    /// </summary>
    public static PasswordMatcher DecorationMatcher { get; } = Any([
        DecorationRunMatcher,
        Sequence([ShortDecorationMatcher, Optional(DecorationRunMatcher)]),
        Sequence([DecorationRunMatcher, ShortDecorationMatcher, Optional(DecorationRunMatcher)]),
    ]);

    private static bool IsSymbol(char c) => char.IsPunctuation(c) || char.IsSymbol(c);

    /// <summary>
    /// Creates a matcher for a common word. Character substitutions are only checked for words longer than two letters, since a substituted
    /// one or two letter word (such as "1" for "i") is far more likely to be a decoration than the word.
    /// </summary>
    private static PasswordMatcher Word(string text) => Text(text, checkSubstitutions: text.Length > 2);
}

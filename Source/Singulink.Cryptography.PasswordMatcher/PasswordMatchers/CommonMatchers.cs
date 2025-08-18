using static Singulink.Cryptography.PasswordMatcher;

namespace Singulink.Cryptography.PasswordMatchers;

/// <summary>
/// Provides predefined matchers for the most common password text.
/// </summary>
public static class CommonMatchers
{
    /// <summary>
    /// Gets a matcher that matches common years (1900 - current + 10).
    /// </summary>
    public static PasswordMatcher YearMatcher { get; } = FixedDigits(4, 1900, DateTime.Now.Year + 10, PasswordMatchType.Common);

    public static PasswordMatcher AdjectiveMatcher { get; } = Any([
        Text("best"),
        Text("worst"),
        Text("good"),
        Text("bad"),
        Text("cool"),
        Text("great"),
        Text("awesome"),
        Text("amazing"),
        Text("lovely"),
        Text("sexy"),
        Text("hot"),
        Text("trusted"),
    ]);

    public static PasswordMatcher AdjectiveToSubjectConjunctionMatcher { get; } = Any([
        Text("like"),
        Text("as"),
    ]);

    public static PasswordMatcher SubjectToAdjectiveDeterminerMatcher { get; } = Any([
        Text("the"),
        Text("a"),
        Text("an"),
    ]);

    public static PasswordMatcher SubjectToAdjectiveModifierMatcher { get; } = Text("so");

    public static PasswordMatcher SubjectToAdjectiveCopularVerbMatcher { get; } = Any([
        Sequence([Text("is"), Optional(SubjectToAdjectiveModifierMatcher), Optional(SubjectToAdjectiveDeterminerMatcher)]),
        Sequence([Text("are"), Optional(SubjectToAdjectiveModifierMatcher), Optional(SubjectToAdjectiveDeterminerMatcher)]),
        Sequence([Text("am"), Optional(SubjectToAdjectiveModifierMatcher), Optional(SubjectToAdjectiveDeterminerMatcher)]),
    ]);

    public static PasswordMatcher SubjectToSubjectCopularVerbMatcher { get; } = Any([
        Text("is"),
        Text("am"),
        Text("are"),
    ]);

    public static PasswordMatcher SubjectConjunctionMatcher { get; } = Any([
        Text("and"),
        Text("or"),
        Text("to"),
        Text("in"), // TODO: is this needed?
        Text("into"),
        Sequence([Text("in"), Text("to")]),
    ]);

    public static PasswordMatcher SubjectDeterminerMatcher { get; } = Any([
        Text("the"),
        Text("a"),
        Text("an"),
        Text("this"),
        Text("my"),
        Text("your"),
        Text("their"),
        Text("trusted"),
        Text("into"),
        Text("let"),
    ]);

    public static PasswordMatcher GeneralSubjectMatcher { get; } = Any([
        Text("i"),
        Text("me"),
        Text("you"),
        Text("this"),

        Text("password"),
        Text("pass"),
        Text("login"),
        Text("secret"),

        Text("welcome"),
        Text("hello"),
        Text("hi"),

        Text("master"),
        Text("admin"),
        Text("user"),
        Text("nobody"),
        Text("no1"),

        Text("football"),
        Text("baseball"),
        Text("hockey"),
        Text("soccer"),

        Text("noob"),
        Text("google"),
        Text("monkey"),
        Text("dragon"),
        Text("ninja"),
        Text("batman"),
        Text("hottie"),
        Text("shadow"),
        Text("sunshine"),
        Text("princess"),
        Text("freedom"),
        Text("jesus"),
        Text("god"),
        Text("whatever"),
        Text("love"),
        Text("sex"),
        Text("fuck"),
        Text("shit"),
        Text("starwars"),
        Sequence([Text("star"), Text("wars")]),

        Text("69", checkSubstitutions: false),
        Text("420", checkSubstitutions: false),
    ]);

    public static PasswordMatcher VerbMatcher { get; } = Any([
        Text("love"),
        Text("like"),
        Text("hate"),
        Text("am"),
        Text("want"),
        Text("trust"),

        Text("login"),
        Text("log"),

        Text("let"),
    ]);

    public static PasswordMatcher VerbSuffixMatcher { get; } = Text("to");

    public static readonly PasswordMatcher PasswordPrefixMatcher = Permutations([
        YearMatcher,
        Sequence([
            KeyboardSequence(KeyboardSequenceTypes.Row),
            Optional(KeyboardSequence(KeyboardSequenceTypes.Row)),
        ]),
        Sequence([
            KeyboardSequence(KeyboardSequenceTypes.Row),
            Optional(KeyboardSequence(KeyboardSequenceTypes.Row)),
        ]),
    ]);

    public static readonly PasswordMatcher PasswordSuffixMatcher = Permutations([
        YearMatcher,
        Sequence([
            KeyboardSequence(KeyboardSequenceTypes.Row),
            Optional(KeyboardSequence(KeyboardSequenceTypes.Row)),
        ]),
        Sequence([
            KeyboardSequence(KeyboardSequenceTypes.Row),
            Optional(KeyboardSequence(KeyboardSequenceTypes.Row)),
        ]),
        Sequence([
            RepeatedChars(1, 8, c => c is >= '0' and <= '9'),
            Optional(RepeatedChars(1, 8, c => c is >= '0' and <= '9')),
        ]),
        RepeatedChars(1, 8, c => c is '!' or '$' or '?'),
    ]);
}

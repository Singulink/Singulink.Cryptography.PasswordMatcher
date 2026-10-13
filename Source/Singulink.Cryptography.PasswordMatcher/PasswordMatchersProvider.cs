using static Singulink.Cryptography.PasswordMatcher;
using static Singulink.Cryptography.PasswordMatchers.CommonMatchers;

namespace Singulink.Cryptography;

/// <summary>
/// Supplies matchers that cover the most easily dictionary-attacked password patterns, built from the common values in
/// <see cref="PasswordMatchers.CommonMatchers"/> and the contextual subjects provided for each password.
/// </summary>
/// <remarks>
/// <para>The patterns are sentence-like combinations of subjects (common words such as "password" and "admin" along with contextual subjects such
/// as the user's name), adjectives, verbs and connecting words, optionally surrounded by common decorations such as years, keyboard sequences and
/// repeated digits or symbols. A final pattern matches any short combination of words and decorations that the sentence-like patterns do not
/// cover. Override <see cref="GetMatchers(IEnumerable{ContextualSubject})"/> to customize the patterns.</para>
/// </remarks>
public class PasswordMatchersProvider : IPasswordMatchersProvider
{
    /// <summary>
    /// The maximum number of consecutive words or structured decorations matched by the final catch-all pattern.
    /// </summary>
    public const int MaxCatchAllTokens = 6;

    /// <summary>
    /// Gets the default provider, which uses <see cref="ContextualSubjectMatcherProvider.Default"/> for contextual subjects.
    /// </summary>
    public static PasswordMatchersProvider Default { get; } = new PasswordMatchersProvider(ContextualSubjectMatcherProvider.Default);

    private readonly ImmutableArray<PasswordMatcher> _applicationSubjectMatchers;

    /// <summary>
    /// Gets the provider used to create matchers for contextual subjects.
    /// </summary>
    public IContextualSubjectMatcherProvider SubjectMatcherProvider { get; }

    /// <summary>
    /// Gets the contextual subjects that apply to every password check, such as the names of the application, company and products, in
    /// addition to the subjects passed to each check.
    /// </summary>
    public ImmutableArray<ContextualSubject> ApplicationSubjects { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PasswordMatchersProvider"/> class.
    /// </summary>
    /// <param name="subjectMatcherProvider">The provider used to create matchers for contextual subjects.</param>
    /// <param name="applicationSubjects">Contextual subjects that apply to every password check, such as the names of the application, company
    /// and products, or <see langword="null"/> if there are none.</param>
    public PasswordMatchersProvider(IContextualSubjectMatcherProvider subjectMatcherProvider, IEnumerable<ContextualSubject>? applicationSubjects = null)
    {
        SubjectMatcherProvider = subjectMatcherProvider;
        ApplicationSubjects = applicationSubjects?.ToImmutableArray() ?? [];

        _applicationSubjectMatchers = ApplicationSubjects
            .Select(subjectMatcherProvider.GetMatcher)
            .OfType<PasswordMatcher>()
            .ToImmutableArray();
    }

    /// <inheritdoc/>
    public virtual IEnumerable<PasswordMatcher> GetMatchers(IEnumerable<ContextualSubject>? subjects)
    {
        // Subject matchers

        var contextualSubjectMatchers = _applicationSubjectMatchers.ToList();

        if (subjects is not null)
            contextualSubjectMatchers.AddRange(subjects.Select(SubjectMatcherProvider.GetMatcher).OfType<PasswordMatcher>());

        var singleSubjectMatcher = Any([
            ..contextualSubjectMatchers,
            GeneralSubjectMatcher,
        ]);

        var subjectMatcher = Sequence([
            singleSubjectMatcher,
            Optional(singleSubjectMatcher),
        ]);

        var subjectWithOptionalDeterminerMatcher = Sequence([
            Optional(SubjectDeterminerMatcher),
            subjectMatcher,
        ]);

        // Adjective matchers

        // Pattern: [prefix] adjective
        // Example: "[is] great"
        var adjectiveAfterSubjectMatcher = Sequence([
            Optional(SubjectToAdjectiveCopularVerbMatcher),
            AdjectiveMatcher,
        ]);

        // Pattern: [determiner] adjective
        // Example: "[the] great"
        var adjectiveBeforeSubjectMatcher = Sequence([
            Optional(SubjectDeterminerMatcher),
            AdjectiveMatcher,
        ]);

        // Verb matchers

        // Pattern: verb [suffix]
        // Example: "loves [to]"
        var verbWithOptionalSuffixMatcher = Sequence([
            VerbMatcher,
            Optional(VerbSuffixMatcher),
        ]);

        // ----------------------------------
        // Resulting common sequence matchers
        // ----------------------------------

        // Pattern: decoration
        // Example: qwerty!!!, 111111, 1qaz2wsx
        yield return DecorationMatcher;

        // Pattern: subject [subject] [subject] [subject]
        // Example: password [password] [password] [password]
        yield return PasswordSequence([
            subjectMatcher, // matches up to 2 consecutive subjects
            Optional(subjectMatcher),
        ]);

        // Pattern: [determiner]subject [[prefix]adjective]
        // Example: [this] password [[is] great]
        yield return PasswordSequence([
            subjectWithOptionalDeterminerMatcher,
            Optional(adjectiveAfterSubjectMatcher),
        ]);

        // Pattern: [determiner]subject (copular verb) [determiner]subject
        // Example: [the] password [is] [a] password
        yield return PasswordSequence([
            subjectWithOptionalDeterminerMatcher,
            SubjectToSubjectCopularVerbMatcher,
            subjectWithOptionalDeterminerMatcher,
        ]);

        // Pattern: [determiner]adjective subject
        // Example: [the] great password
        yield return PasswordSequence([
            adjectiveBeforeSubjectMatcher,
            subjectMatcher,
        ]);

        // Pattern: [determiner]subject [prefix]adjective [determiner]subject
        // Example: this [is a] bad password
        yield return PasswordSequence([
            subjectWithOptionalDeterminerMatcher,
            adjectiveAfterSubjectMatcher,
            subjectWithOptionalDeterminerMatcher,
        ]);

        // Pattern: [[determiner]subject [copular verb]] adjective [conjunction] [determiner]subject
        // Example: [i [am]] cool as [a] ninja
        yield return PasswordSequence([
            Optional(Sequence([subjectWithOptionalDeterminerMatcher, SubjectToSubjectCopularVerbMatcher])),
            Sequence([AdjectiveMatcher, Optional(AdjectiveToSubjectConjunctionMatcher)]),
            subjectWithOptionalDeterminerMatcher,
        ]);

        // Pattern: [determiner]subject [conjunction] [determiner]subject [[prefix]adjective]
        // Example: [this] password [and] [the] appname [[is] great]
        yield return PasswordSequence([
            subjectWithOptionalDeterminerMatcher,
            Optional(SubjectConjunctionMatcher),
            subjectWithOptionalDeterminerMatcher,
            Optional(adjectiveAfterSubjectMatcher),
        ]);

        // Pattern: [determiner]subject verb[suffix] verb
        // Example: [this] admin loves [to] login
        yield return PasswordSequence([
            subjectWithOptionalDeterminerMatcher,
            verbWithOptionalSuffixMatcher,
            VerbMatcher,
        ]);

        // Pattern: [determiner]subject verb[suffix] [verb[suffix]] [determiner]subject
        // Example: [this] admin loves [to] log in[to] appname
        yield return PasswordSequence([
            subjectWithOptionalDeterminerMatcher,
            verbWithOptionalSuffixMatcher,
            Optional(verbWithOptionalSuffixMatcher),
            subjectWithOptionalDeterminerMatcher,
        ]);

        // Catch-all pattern: up to 6 words or structured decorations in any order, where a word may be joined to whatever precedes it by a
        // single digit or symbol
        // Example: cool 2024 admin, mike 123 acme, love you, ninja !admin
        var wordMatcher = Any([
            ..contextualSubjectMatchers,
            AllWordsMatcher,
        ]);

        var tokenMatcher = Any([
            Sequence([Optional(JoinerMatcher), wordMatcher]),
            StructuredDecorationMatcher,
        ]);

        yield return PasswordSequence([
            Repeat(tokenMatcher, 1, MaxCatchAllTokens),
        ]);

        static PasswordMatcher PasswordSequence(IEnumerable<PasswordMatcher> matchers)
        {
            return Sequence([
                Optional(DecorationMatcher),
                ..matchers,
                Optional(DecorationMatcher)
            ]);
        }
    }
}
